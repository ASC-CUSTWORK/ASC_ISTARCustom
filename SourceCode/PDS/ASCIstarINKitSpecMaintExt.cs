using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using PX.Objects.AP;
using PX.Objects.PO;
using System.Collections;
using System.Reflection;
using ASCISTARCustom.PDS.CacheExt;
using ASCISTARCustom.Inventory.DAC;
using ASCISTARCustom.Inventory.CacheExt;
using ASCISTARCustom.Cost.Descriptor;
using ASCISTARCustom.Common.Helper;
using System;
using PX.Objects.FA;
using static PX.CS.RMReport.FK;
using ASCISTARCustom.Common.Builder;
using System.Linq;

namespace ASCISTARCustom.PDS
{
    public class ASCIStarINKitSpecMaintExt : PXGraphExtension<INKitSpecMaint>
    {
        #region Static Functions
        public static bool IsActive() => true;
        #endregion

        #region View
        public SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<INKitSpecHdr.kitInventoryID.FromCurrent>>.View InventoryItemHdr;
                
        public PXSelect<APVendorPrice, Where<APVendorPrice.inventoryID, Equal<Current<INKitSpecStkDet.compInventoryID>>,
            And<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>>>> iStarCommodityPrice;

        public PXSelect<POVendorInventory, Where<POVendorInventory.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>> VendorItems;

        public PXSelect<POVendorInventory, Where<POVendorInventory.inventoryID, Equal<Required<INKitSpecHdr.kitInventoryID>>, And<POVendorInventory.isDefault, Equal<True>>>> DefaultVendorItem;

        public PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<INKitSpecStkDet.compInventoryID>>>> BaseItem;

        public SelectFrom<ASCIStarINKitSpecJewelryItem>
                 .Where<ASCIStarINKitSpecJewelryItem.kitInventoryID.IsEqual<INKitSpecHdr.kitInventoryID.FromCurrent>
                    .And<ASCIStarINKitSpecJewelryItem.revisionID.IsEqual<INKitSpecHdr.revisionID.FromCurrent>>>
                        .View JewelryItemView;

        [PXFilterable]
        public PXSelectJoin<APVendorPrice,
            InnerJoin<POVendorInventory, On<POVendorInventory.vendorID, Equal<Current<APVendorPrice.vendorID>>,
                And<POVendorInventory.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>>,
            InnerJoin<InventoryItemCurySettings, On<InventoryItemCurySettings.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
                And<InventoryItemCurySettings.preferredVendorID, Equal<POVendorInventory.vendorID>>>,
            InnerJoin<InventoryItem, On<APVendorPrice.inventoryID, Equal<InventoryItem.inventoryID>>,
            InnerJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>>>>,
        Where<APVendorPrice.vendorID, Equal<InventoryItemCurySettings.preferredVendorID>,
            And<INItemClass.itemClassCD, Equal<CommodityClass>,
            And<APVendorPrice.effectiveDate, LessEqual<AccessInfo.businessDate>,
            And<APVendorPrice.expirationDate, GreaterEqual<AccessInfo.businessDate>>>>>,
        OrderBy<Desc<APVendorPrice.effectiveDate>>> VendorPriceBasis;

        [PXFilterable]
        public SelectFrom<APVendorPrice>.
            InnerJoin<POVendorInventory>.
            On<POVendorInventory.inventoryID.IsEqual<INKitSpecHdr.kitInventoryID.FromCurrent>.
                And<APVendorPrice.vendorID.IsEqual<POVendorInventory.vendorID>>>.
            InnerJoin<InventoryItem>.
                On<InventoryItem.inventoryID.IsEqual<APVendorPrice.inventoryID>>.
            InnerJoin<INItemClass>.On<InventoryItem.itemClassID.IsEqual<INItemClass.itemClassID>.
                And<INItemClass.itemClassCD.IsEqual<CommodityClass>>>.
            Where<APVendorPrice.effectiveDate.IsLessEqual<AccessInfo.businessDate>.
                And<Brackets<APVendorPrice.expirationDate.IsGreaterEqual<AccessInfo.businessDate>.
                    Or<APVendorPrice.expirationDate.IsNull>>>>.
                    OrderBy<APVendorPrice.effectiveDate.Asc> MarketPriceBasis;

        [PXFilterable]
        public PXSelectJoin<POVendorInventory,
                InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>>>> VendorContractPrice;

        public PXSelect<INKitSpecStkDet,
                Where<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
                And<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
                And<INKitSpecStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>>>
            SpecComponents;

        public PXSelectJoin<INKitSpecStkDet,
                InnerJoin<InventoryItem, On<INKitSpecStkDet.FK.ComponentInventoryItem>,
                InnerJoin<INItemClass, On<InventoryItem.FK.ItemClass>>>,
                Where<INItemClass.itemClassCD, Equal<CommodityClass>,
                And<INKitSpecStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
                And<INKitSpecStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>>>
            SpecCommodity;


        public PXSelectJoin<INKitSpecNonStkDet,
                InnerJoin<InventoryItem, On<INKitSpecNonStkDet.FK.ComponentInventoryItem>>,
                Where<INKitSpecNonStkDet.kitInventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
                And<INKitSpecNonStkDet.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>>
            SpecOverhead;

        public PXSelect<INUnit,
                Where<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>,
                And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>>>>
            CommodityConversion;

        public PXSelect<
            ASCIStarItemWeightCostSpec,
            Where<ASCIStarItemWeightCostSpec.inventoryID, Equal<Current<INKitSpecHdr.kitInventoryID>>,
                And<ASCIStarItemWeightCostSpec.revisionID, Equal<Current<INKitSpecHdr.revisionID>>>>>
            ASCIStarCostSpecification;
        #endregion

        #region CacheAttached
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<APVendorPrice.salesPrice, TOZ2GRAM>>))]
        protected void APVendorPrice_UsrCommodityPerGram_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<APVendorPrice.salesPrice, ASCIStarAPVendorPriceExt.usrCommodityPrice>>))]
        protected void APVendorPrice_UsrIncrement_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXFormula(typeof(Switch<Case<Where<Current<APVendorPrice.uOM>, NotEqual<TOZ>>, Null>, Div<Div<APVendorPrice.salesPrice, ASCIStarAPVendorPriceExt.usrCommodityPrice>, TOZ2GRAM>>))]
        protected void APVendorPrice_UsrIncrementPerGram_CacheAttached(PXCache sender) { }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault(typeof(INKitSpecHdr.kitInventoryID))]
        protected void APVendorPrice_InventoryID_CacheAttached(PXCache sender) { }

        [PXRemoveBaseAttribute(typeof(PXDBDefaultAttribute))]
        [PXRemoveBaseAttribute(typeof(PXParentAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDBDefault(typeof(INKitSpecHdr.kitInventoryID))]
        [PXParent(typeof(ASCIStarItemWeightCostSpec.FK.InventoryItemFK))]
        protected void _(Events.CacheAttached<ASCIStarItemWeightCostSpec.inventoryID> cacheAttached) { }

        [PXMergeAttributes(Method = MergeMethod.Replace)]
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXDBDefault(typeof(INKitSpecHdr.revisionID))]
        [PXParent(typeof(ASCIStarItemWeightCostSpec.FK.KitSpecificationFK))]
        protected void _(Events.CacheAttached<ASCIStarItemWeightCostSpec.revisionID> cacheAttached) { }
        #endregion

        #region Actions
        public PXAction<INKitSpecHdr> createitem;
        [PXUIField(DisplayName = "Create Production Item", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable createItem(PXAdapter adapter)
        {
            return adapter.Get();
        }
        #endregion

        #region Event Handlers
        protected void INKitSpecStkDet_DfltCompQty_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);
            var row = (INKitSpecStkDet)e.Row;
            if (row == null) return;
            ASCIStarINKitSpecStkDetExt stkDetExt = cache.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
            if (stkDetExt == null) return;
            cache.SetValueExt<ASCIStarINKitSpecStkDetExt.usrExtCost>(cache.Current, row.DfltCompQty * stkDetExt.UsrUnitCost);
            InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
            INItemClass itemClass = INItemClass.PK.Find(cache.Graph, item.ItemClassID);

            PXTrace.WriteInformation($"item.InventoryCD:{item.InventoryCD.Trim()}");
            PXTrace.WriteInformation($"itemClass.ItemClassCD:{itemClass.ItemClassCD.Trim()}");

            if (itemClass.ItemClassCD.Trim() == CommodityClass.value)
            {
                ASCIStarINInventoryItemExt itemExt = item.GetExtension<ASCIStarINInventoryItemExt>();
                InventoryItem priceAsItem = InventoryItem.PK.Find(cache.Graph, itemExt.UsrPriceAsID);
                PXTrace.WriteInformation($"priceAsItem.InventoryCD:{priceAsItem.InventoryCD.Trim()}");
                ASCIStarINInventoryItemExt priceAsUnit = priceAsItem.GetExtension<ASCIStarINInventoryItemExt>();

                decimal WeightRollup = 0.00m;

                //if (row.UOM == "")
                //{
                decimal rowWeight = 0.00m;
                rowWeight = row.DfltCompQty.Value;
                if (row.UOM == "DWT")
                    rowWeight *= 1.555174m;
                WeightRollup += rowWeight;
                //}

                PXTrace.WriteInformation($"WeightRollup:{WeightRollup} to be converted {priceAsUnit.UsrPriceToUnit.Trim()}/{priceAsItem.InventoryCD.Trim()}");

                INUnit inUnit =
                    new PXSelect<INUnit,
                    Where<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>,
                        And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>>>>(cache.Graph).SelectSingle(
                        priceAsUnit.UsrPriceToUnit.Trim(),
                        priceAsItem.InventoryCD.Trim());

                if (inUnit == null) return;
                PXTrace.WriteInformation($"inUnit.UnitRate:{inUnit.UnitRate}");

                decimal convert = (decimal)inUnit.UnitRate;

                if (priceAsItem.InventoryCD.Trim() == "SSS")
                {
                    cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(cache.Current, WeightRollup);  //stkDetExt.UsrExtCost = row.DfltCompQty * stkDetExt.UsrUnitCost;
                    cache.SetValueExt<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(cache.Current, WeightRollup * convert);  //stkDetExt.UsrExtCost = row.DfltCompQty * stkDetExt.UsrUnitCost;
                    cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(e.Row, null);
                    cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(e.Row, null);
                }
                if (priceAsItem.InventoryCD.Trim() == "24K")
                {
                    cache.SetValueExt<ASCIStarINInventoryItemExt.usrActualGRAMGold>(cache.Current, WeightRollup);  //stkDetExt.UsrExtCost = row.DfltCompQty * stkDetExt.UsrUnitCost;
                    cache.SetValueExt<ASCIStarINInventoryItemExt.usrPricingGRAMGold>(cache.Current, WeightRollup * convert);  //stkDetExt.UsrExtCost = row.DfltCompQty * stkDetExt.UsrUnitCost;
                    cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrActualGRAMSilver>(e.Row, null);
                    cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrPricingGRAMSilver>(e.Row, null);
                }
            }
        }

        protected void INKitSpecHdr_UsrUnitCost_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e, PXFieldDefaulting InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            e.NewValue = 0.00m;
            INKitSpecStkDet row = e.Row as INKitSpecStkDet;
            if (row == null)
                return;
        }
        
        protected void INKitSpecStkDet_UsrCostingType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e, PXFieldDefaulting InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            e.NewValue = ASCIStarCostingType.StandardCost;
            INKitSpecStkDet row = e.Row as INKitSpecStkDet;
            if (row == null) return;
            InventoryItem baseItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<INKitSpecStkDet.compInventoryID>>>>.Select(cache.Graph);
            if (baseItem == null) return;
            ASCIStarINInventoryItemExt extItem = baseItem.GetExtension<ASCIStarINInventoryItemExt>();
            if (extItem == null || extItem.UsrCostingType == null) return;

            e.NewValue = extItem.UsrCostingType;

        }
       
        protected void INKitSpecStkDet_UsrCostingType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            var row = (INKitSpecStkDet)e.Row;
            if (row == null) return;

            ASCIStarINKitSpecStkDetExt ext = cache.GetExtension<ASCIStarINKitSpecStkDetExt>(row);
            PXTrace.WriteInformation($"ext.UsrCostingType:{ext.UsrCostingType}");
            if (/*ext.UsrCostingType == CostingType.PercentageCost*/ false)
            {
                PXTrace.WriteInformation($"Disabling Cost");
                ext.UsrUnitCost = 0.00m;
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecStkDetExt.usrUnitCost>(cache, e.Row, false);
                ext.UsrUnitPct = 0.00m;
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecStkDetExt.usrUnitPct>(cache, e.Row, true);
            }
            else
            {
                PXTrace.WriteInformation($"Enabling Percentage");
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecStkDetExt.usrUnitCost>(cache, e.Row, true);
                PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecStkDetExt.usrUnitPct>(cache, e.Row, false);
            }
        }

        protected void INKitSpecStkDet_UsrUnitCost_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            var row = (INKitSpecStkDet)e.Row;
            if (row == null) return;
            ASCIStarINKitSpecStkDetExt ext = cache.GetExtension<ASCIStarINKitSpecStkDetExt>(row);

            ext.UsrExtCost = ext.UsrUnitCost * row.DfltCompQty;
        }

        protected void INKitSpecStkDet_UsrExtCost_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            var row = (INKitSpecStkDet)e.Row;
            if (row == null) return;

            InventoryItem baseItem = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
            if (baseItem == null) return;
            ASCIStarINInventoryItemExt ext = baseItem.GetExtension<ASCIStarINInventoryItemExt>();
            if (ext == null) return;
            decimal Rollup = 0.0m;

            if (e.Row != null)
            {
                PXTrace.WriteInformation($"{ext.UsrCostRollupType}");
                foreach (PXResult<INKitSpecStkDet> rec in
                    SpecComponents.Select(cache))
                {
                    INKitSpecStkDet StkDet = (INKitSpecStkDet)rec;

                    InventoryItem compItem = InventoryItem.PK.Find(cache.Graph, StkDet.CompInventoryID);
                    ASCIStarINInventoryItemExt compItemExt = compItem.GetExtension<ASCIStarINInventoryItemExt>();

                    if (compItemExt.UsrCostRollupType == ext.UsrCostRollupType)
                    {
                        ASCIStarINKitSpecStkDetExt StkDetExt = StkDet.GetExtension<ASCIStarINKitSpecStkDetExt>();
                        Rollup += StkDetExt.UsrExtCost.Value;
                    }
                    PXTrace.WriteInformation($"Rollup:{Rollup}");
                }

                InventoryItem item = InventoryItemHdr.SelectSingle();
                ext = item.GetExtension<ASCIStarINInventoryItemExt>();
                /*Commodity, Fabrication, Labor, Handling, Shipping, Duty, Other*/
                switch (ext.UsrCostRollupType)
                {
                    case ASCIStarCostRollupType.Commodity:
                        ext.UsrCommodityCost = Rollup;
                        break;
                    case ASCIStarCostRollupType.Fabrication:
                        ext.UsrFabricationCost = Rollup;
                        break;
                    case ASCIStarCostRollupType.Materials:
                        ext.UsrOtherMaterialCost = Rollup;
                        break;
                    case ASCIStarCostRollupType.Packaging:
                        ext.UsrPackagingCost = Rollup;
                        break;
                    case ASCIStarCostRollupType.Labor:
                        ext.UsrLaborCost = Rollup;
                        break;
                    case ASCIStarCostRollupType.Handling:
                        ext.UsrHandlingCost = Rollup;
                        break;
                    case ASCIStarCostRollupType.Duty:
                        ext.UsrDutyCost = Rollup;
                        break;
                    case ASCIStarCostRollupType.Other:
                        ext.UsrOtherCost = Rollup;
                        break;
                    default: /* CostingType.StandardCost */
                        break;
                }
            }
        }
        
        protected void INKitSpecNonStkDet_UsrExtCost_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            var row = (INKitSpecNonStkDet)e.Row;
            if (row == null) return;

            InventoryItem baseItem = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
            if (baseItem == null) return;
            ASCIStarINInventoryItemExt ext = baseItem.GetExtension<ASCIStarINInventoryItemExt>();
            if (ext == null) return;

            decimal Rollup = 0.0m;

            if (e.Row != null)
            {

                PXTrace.WriteInformation($"{ext.UsrCostRollupType}");
                foreach (PXResult<INKitSpecNonStkDet> rec in
                    SpecOverhead.Select(cache))
                {
                    INKitSpecNonStkDet StkDet = (INKitSpecNonStkDet)rec;

                    InventoryItem compItem = InventoryItem.PK.Find(cache.Graph, StkDet.CompInventoryID);
                    ASCIStarINInventoryItemExt compItemExt = compItem.GetExtension<ASCIStarINInventoryItemExt>();

                    if (compItemExt.UsrCostRollupType == ext.UsrCostRollupType)
                    {
                        ASCIStarINKitSpecNonStkDetExt StkDetExt = StkDet.GetExtension<ASCIStarINKitSpecNonStkDetExt>();
                        Rollup += StkDetExt.UsrExtCost.Value;
                    }
                    PXTrace.WriteInformation($"Rollup:{Rollup}");
                }

                /*Commodity, Fabrication, Labor, Handling, Shipping, Duty, Other*/
                switch (ext.UsrCostRollupType)
                {
                    case ASCIStarCostRollupType.Commodity:
                        ext.UsrCommodityCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrCommodityCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Fabrication:
                        ext.UsrFabricationCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrFabricationCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Materials:
                        ext.UsrOtherMaterialCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrOtherMaterialCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Packaging:
                        ext.UsrPackagingCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrPackagingCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Labor:
                        ext.UsrLaborCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrLaborCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Handling:
                        ext.UsrHandlingCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrHandlingCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Duty:
                        ext.UsrDutyCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrDutyCost>(e.Row, null);
                        break;
                    case ASCIStarCostRollupType.Other:
                        ext.UsrOtherCost = Rollup;
                        cache.RaiseFieldUpdated<ASCIStarINInventoryItemExt.usrOtherCost>(e.Row, null);
                        break;
                    default: /* CostingType.StandardCost */
                        break;
                }

            }
        }
        
        protected void INKitSpecNonStkDet_UsrCostingType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);
            var row = (INKitSpecNonStkDet)e.Row;
            if (row == null) return;

            ASCIStarINKitSpecNonStkDetExt ext = row.GetExtension<ASCIStarINKitSpecNonStkDetExt>();
            PXTrace.WriteInformation($"e.NewValue:{ext.UsrCostingType}");
            if (ASCIStarCostingType.WeightCost == (string)ext.UsrCostingType)
            {
                row.DfltCompQty = 1.00m;
                decimal qty = 0.00m;
                row.UOM = "GRAM";
                foreach (INKitSpecStkDet r in
                    SpecCommodity.Select(cache))
                {
                    InventoryItem item = InventoryItem.PK.Find(cache.Graph, r.CompInventoryID);
                    PXTrace.WriteInformation($"item:{item.InventoryCD}");
                    if (r.UOM == "DWT")
                        qty += ((r.DfltCompQty ?? 0.00m) * 1.555170m);
                    else
                        qty += (r.DfltCompQty ?? 0.00m);

                }
                row.DfltCompQty = qty;
            }
            else
                row.UOM = "EA";


        }
       
        protected void INKitSpecStkDet_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);
            var row = (INKitSpecStkDet)e.Row;
            if (row == null) return;
            InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
            InventoryItem kit = InventoryItem.PK.Find(cache.Graph, row.KitInventoryID);
            PXResultset<INKitSpecHdr> iNKitSpecHdr = PXSelect<INKitSpecHdr, Where<INKitSpecHdr.kitInventoryID, Equal<Current<INKitSpecStkDet.kitInventoryID>>>>.SelectWindowed(cache.Graph, 0, 2);

            ASCIStarINInventoryItemExt ext = item.GetExtension<ASCIStarINInventoryItemExt>();
            PXTrace.WriteInformation($"Kit:{kit.InventoryCD}");
            PXTrace.WriteInformation($"Component Item:{item.InventoryCD}");
            //ASCIStarMarketCostProvider.JewelryCost itemCost =
            //    new ASCIStarMarketCostProvider.JewelryCost(cache.Graph, item, 0.00m);

            ext.UsrUnitCost = 0.000000m;
            //ext.UsrCostingType = itemCost.costingType;
            //ext.UsrCostRollupType = itemCost.costRollupType;
            //ext.UsrExtCost = ext.UsrUnitCost * row.DfltCompQty;

            INItemClass itemClass = INItemClass.PK.Find(cache.Graph, item.ItemClassID);
            if (itemClass.ItemClassCD == CommodityClass.value)
            {
                ext = item.GetExtension<ASCIStarINInventoryItemExt>();
                if (ext == null)
                    return;
                ext.UsrContractWgt = row.DfltCompQty;
            }
        }

        protected virtual void INKitSpecNonStkDet_UsrCostingType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e, PXFieldDefaulting InvokeBaseHandler)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            PXTrace.WriteInformation("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);

            INKitSpecNonStkDet row = (INKitSpecNonStkDet)e.Row;
            if (row == null)
                return;
            InventoryItem item = InventoryItem.PK.Find(cache.Graph, row.CompInventoryID);
            if (item == null)
            {
                e.NewValue = ASCIStarCostingType.StandardCost;
                return;
            }
            ASCIStarINInventoryItemExt itemExt = item.GetExtension<ASCIStarINInventoryItemExt>();
            e.NewValue = itemExt.UsrCostingType ?? ASCIStarCostingType.StandardCost;
            PXUIFieldAttribute.SetEnabled<ASCIStarINKitSpecNonStkDetExt.usrCostingType>(cache, row, true);
        }

        #region INKitSpecHdr Events
        protected virtual void _(Events.RowInserted<INKitSpecHdr> e)
        {
            var row = e.Row;
            if (row == null || this.Base.Hdr.Current == null) return;

            CopyJewelryItemFields(this.Base.Hdr.Current);
        }

        protected virtual void _(Events.RowSelected<INKitSpecHdr> e, PXRowSelected baseMethod)
        {
            if (e.Row is INKitSpecHdr row)
            {
                SetVisibleRevisionID();

                var baseMetalType = ASCIStarMetalType.GetMetalType(this.JewelryItemView.Current?.MetalType);
                SetReadOnlyItemCostSpecificationFields(e.Cache, ASCIStarCostSpecification.Current, baseMetalType);
            }
        }
        #endregion

        #region ASCIStarItemWeightCostSpec Events
        protected virtual void _(Events.FieldUpdated<ASCIStarItemWeightCostSpec, ASCIStarItemWeightCostSpec.goldGrams> e)
        {
            if (e.Row is ASCIStarItemWeightCostSpec row)
            {
                if (JewelryItemView.Current != null)
                {
                    var value = ASCIStarMetalType.GetGoldTypeValue(JewelryItemView.Current?.MetalType);
                    var result = row?.GoldGrams * (value / 24);

                    e.Cache.SetValueExt<ASCIStarItemWeightCostSpec.fineGoldGrams>(row, result);
                }
            }
        }
        protected virtual void _(Events.FieldUpdated<ASCIStarItemWeightCostSpec, ASCIStarItemWeightCostSpec.fineGoldGrams> e)
        {
            if (e.Row is ASCIStarItemWeightCostSpec row)
            {
                UpdateCommodityCostMetal(e.Cache, Base.Hdr.Current, row);
                var value = ASCIStarMetalType.GetGoldTypeValue(this.JewelryItemView.Current?.MetalType);
                var result = (decimal?)e.NewValue * 24 / value;
                if (result != row.GoldGrams)
                {
                    row.GoldGrams = result;
                }
            }
        }
        #endregion

        #region JewelryItem Events
        protected virtual void _(Events.FieldUpdated<ASCIStarINKitSpecJewelryItem, ASCIStarINKitSpecJewelryItem.metalType> e)
        {
            if (e.Row is ASCIStarINKitSpecJewelryItem row)
            {
                var result = ASCIStarMetalType.GetMetalType(this.JewelryItemView.Current?.MetalType);
                SetMetalGramsToZero(result, e.Cache, this.ASCIStarCostSpecification.Current);
            }
        }
        #endregion

        #endregion

        #region ServiceMethods
        private void CopyJewelryItemFields(INKitSpecHdr kitSpecHdr)
        {
            var jewelItem = SelectFrom<ASCIStarINJewelryItem>.Where<ASCIStarINJewelryItem.inventoryID.IsEqual<PX.Data.BQL.P.AsInt>>.View.Select(this.Base, kitSpecHdr.KitInventoryID)?.TopFirst;

            if (jewelItem == null) return;

            ASCIStarINKitSpecJewelryItem jewelryKitItem = new ASCIStarINKitSpecJewelryItem()
            {
                KitInventoryID = kitSpecHdr.KitInventoryID,
                RevisionID = kitSpecHdr.RevisionID,
                ShortDesc = jewelItem.ShortDesc,
                LongDesc = jewelItem.LongDesc,
                StyleStatus = jewelItem.StyleStatus,
                CustomerCode = jewelItem.CustomerCode,
                InvCategory = jewelItem?.InvCategory,
                ItemType = jewelItem?.ItemType,
                ItemSubType = jewelItem?.ItemSubType,
                Collection = jewelItem.Collection,
                MetalType = jewelItem?.MetalType,
                MetalNote = jewelItem.MetalNote,
                MetalColor = jewelItem?.MetalColor,
                Plating = jewelItem.Plating,
                Finishes = jewelItem.Finishes,
                VendorMaker = jewelItem.VendorMaker,
                OrgCountry = jewelItem?.OrgCountry,
                StoneType = jewelItem.StoneType,
                WebNotesComment = jewelItem.WebNotesComment,
                StoneComment = jewelItem.StoneComment,
                StoneColor = jewelItem?.StoneColor,
                StoneShape = jewelItem.StoneShape,
                StoneCreation = jewelItem.StoneCreation,
                GemstoneTreatment = jewelItem.GemstoneTreatment,
                SettingType = jewelItem.SettingType,
                Findings = jewelItem.Findings,
                FindingsSubType = jewelItem.FindingsSubType,
                ChainType = jewelItem.ChainType,
                RingLength = jewelItem.RingLength,
                RingSize = jewelItem.RingSize,
                OD = jewelItem.OD,
            };

            this.JewelryItemView.Insert(jewelryKitItem);
        }
        private void SetVisibleRevisionID()
        {
            var inSetup = SelectFrom<INSetup>.View.Select(this.Base)?.TopFirst;
            var inSetupExt = inSetup?.GetExtension<ASCIStarINSetupExt>();
            PXUIFieldAttribute.SetVisible<INKitSpecHdr.revisionID>(this.Base.Hdr.Cache, this.Base.Hdr.Current, inSetupExt?.UsrIsPDSTenant == true);
        }
        
        private void SetReadOnlyItemCostSpecificationFields(PXCache cache, ASCIStarItemWeightCostSpec current, bool? baseMetalType)
        {
            if (baseMetalType == null)
            {
                PXUIFieldAttribute.SetReadOnly<ASCIStarItemWeightCostSpec.fineGoldGrams>(cache, current, true);
                PXUIFieldAttribute.SetReadOnly<ASCIStarItemWeightCostSpec.fineSilverGrams>(cache, current, true);
                PXUIFieldAttribute.SetReadOnly<ASCIStarItemWeightCostSpec.goldGrams>(cache, current, true);
                PXUIFieldAttribute.SetReadOnly<ASCIStarItemWeightCostSpec.silverGrams>(cache, current, true);
            }
            else
            {
                bool isReadOnly = baseMetalType == true;
                PXUIFieldAttribute.SetReadOnly<ASCIStarItemWeightCostSpec.goldGrams>(cache, current, !isReadOnly);
                PXUIFieldAttribute.SetReadOnly<ASCIStarItemWeightCostSpec.fineGoldGrams>(cache, current, !isReadOnly);
                PXUIFieldAttribute.SetReadOnly<ASCIStarItemWeightCostSpec.silverGrams>(cache, current, isReadOnly);
                PXUIFieldAttribute.SetReadOnly<ASCIStarItemWeightCostSpec.fineSilverGrams>(cache, current, isReadOnly);
                PXUIFieldAttribute.SetReadOnly<ASCIStarItemWeightCostSpec.metalLossPct>(cache, current, !isReadOnly);
            }
        }

        private void SetMetalGramsToZero(bool? baseMetalType, PXCache cache, ASCIStarItemWeightCostSpec current)
        {
            switch (baseMetalType)
            {
                case true:
                    {
                        cache.SetValueExt<ASCIStarItemWeightCostSpec.silverGrams>(current, decimal.Zero);
                        break;
                    }
                case false:
                    {
                        cache.SetValueExt<ASCIStarItemWeightCostSpec.goldGrams>(current, decimal.Zero);
                        cache.SetValueExt<ASCIStarItemWeightCostSpec.increment>(current, 0.5m);
                        break;
                    }
                default:
                    {
                        cache.SetValueExt<ASCIStarItemWeightCostSpec.silverGrams>(current, decimal.Zero);
                        cache.SetValueExt<ASCIStarItemWeightCostSpec.goldGrams>(current, decimal.Zero);
                        break;
                    }
            }
            cache.SetValueExt<ASCIStarItemWeightCostSpec.landedCost>(current, decimal.Zero);
            cache.SetValueExt<ASCIStarItemWeightCostSpec.unitCost>(current, decimal.Zero);
            cache.SetValueExt<ASCIStarItemWeightCostSpec.preciousMetalCost>(current, decimal.Zero);
            cache.SetValueExt<ASCIStarItemWeightCostSpec.metalLossPct>(current, decimal.Zero);
            cache.SetValueExt<ASCIStarItemWeightCostSpec.surchargePct>(current, decimal.Zero);
        }

        private void UpdateCommodityCostMetal(PXCache cache, INKitSpecHdr kitSpecHdr, ASCIStarItemWeightCostSpec current)
        {
            if ((current.SilverGrams == null || current.SilverGrams == 0.0m) && (current.GoldGrams == null || current.GoldGrams == 0.0m)) return;
            
            var jewelryCostProvider = CreateCostBuilder(current);

            current.PreciousMetalCost = jewelryCostProvider.CalculatePreciousMetalCost();
            cache.SetValueExt<ASCIStarItemWeightCostSpec.preciousMetalCost>(current, current.PreciousMetalCost);

            current.Increment = jewelryCostProvider.CalculateGoldIncrementValue(current);
        }
        private ASCIStarCostBuilder CreateCostBuilder(ASCIStarItemWeightCostSpec currentRow)
        {
            var defaultVendor = VendorItems.Select().RowCast<POVendorInventory>().FirstOrDefault(_ => _.IsDefault == true);
            if (defaultVendor != null)
            {
                return new ASCIStarCostBuilder(this.Base)
                            .WithInventoryItem(currentRow)
                            .WithPOVendorInventory(defaultVendor)
                            .WithINJewelryItem(this.JewelryItemView.Current)
                            .Build();
            }

            throw new PXSetPropertyException("No default vendor on Vendors tab");
        }
        #endregion Helpers Methods
    }
}