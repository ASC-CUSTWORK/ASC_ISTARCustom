﻿using ASCISTARCustom.Common.Descriptor;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using System;
using static ASCISTARCustom.Common.Descriptor.ASCIStarConstants;

namespace ASCISTARCustom.Cost.CacheExt
{
    public sealed class ASCIStarPOVendorInventoryExt : PXCacheExtension<POVendorInventory>
    {
        public static bool IsActive() => true;

        #region UsrMarketID
        [PXDBInt()]
        [PXUIField(DisplayName = "Market")]
        [PXSelector(typeof(Search2<Vendor.bAccountID, InnerJoin<VendorClass, On<Vendor.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<VendorClass.vendorClassID, Equal<MarketClass>>>),
            typeof(Vendor.acctCD), typeof(Vendor.acctName)
            , SubstituteKey = typeof(Vendor.acctCD), DescriptionField = typeof(Vendor.acctName))]
        public int? UsrMarketID { get; set; }
        public abstract class usrMarketID : PX.Data.BQL.BqlInt.Field<usrMarketID> { }
        #endregion

        #region UsrCommodityID
        [PXDBInt()]
        [PXUIField(DisplayName = "Metal")]
        [PXSelector(typeof(Search2<InventoryItem.inventoryID, InnerJoin<INItemClass, On<InventoryItem.itemClassID, Equal<INItemClass.itemClassID>>>, Where<INItemClass.itemClassCD, Equal<ASCIStarConstants.CommodityClass>>>),
            typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr)
            , SubstituteKey = typeof(InventoryItem.inventoryCD), DescriptionField = typeof(InventoryItem.descr))]
        public int? UsrCommodityID { get; set; }
        public abstract class usrCommodityID : PX.Data.BQL.BqlInt.Field<usrCommodityID> { }
        #endregion

        #region UsrIsOverrideVendor
        [PXDBBool()]
        [PXUIField(DisplayName = "Override Vendor")]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? UsrIsOverrideVendor { get; set; }
        public abstract class usrIsOverrideVendor : PX.Data.BQL.BqlBool.Field<usrIsOverrideVendor> { }
        #endregion

        #region UsrCommodityPrice
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Vendor Price")]
        public decimal? UsrCommodityPrice { get; set; }
        public abstract class usrCommodityPrice : PX.Data.BQL.BqlDecimal.Field<usrCommodityPrice> { }
        #endregion

        #region UsrCommodityIncrement
        [PXDBDecimal(6)]
        [PXUIField(DisplayName = "Market Increment")]
        public decimal? UsrCommodityIncrement { get; set; }
        public abstract class usrCommodityIncrement : PX.Data.BQL.BqlDecimal.Field<usrCommodityIncrement> { }
        #endregion

        #region UsrCommodityLossPct
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Metal Loss %", Visible = false)]
        public decimal? UsrCommodityLossPct { get; set; }
        public abstract class usrCommodityLossPct : PX.Data.BQL.BqlDecimal.Field<usrCommodityLossPct> { }
        #endregion

        #region UsrCommoditySurchargePct
        [PXDBDecimal(4)]
        [PXUIField(DisplayName = "Surcharge %", Visible = false)]
        public decimal? UsrCommoditySurchargePct { get; set; }
        public abstract class usrCommoditySurchargePct : PX.Data.BQL.BqlDecimal.Field<usrCommoditySurchargePct> { }
        #endregion

        #region UsrCommodityCost
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Precious Metal Cost")]
        public Decimal? UsrCommodityCost { get; set; }
        public abstract class usrCommodityCost : PX.Data.BQL.BqlDecimal.Field<usrCommodityCost> { }
        #endregion

        #region UsrOtherMaterialCost
        [PXUIField(DisplayName = "Other Material Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        public Decimal? UsrOtherMaterialCost { get; set; }
        public abstract class usrOtherMaterialCost : PX.Data.BQL.BqlDecimal.Field<usrOtherMaterialCost> { }
        #endregion

        #region UsrFabricationCost
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Fabrication/Value Add")]
        public Decimal? UsrFabricationCost { get; set; }
        public abstract class usrFabricationCost : PX.Data.BQL.BqlDecimal.Field<usrFabricationCost> { }
        #endregion

        #region UsrPackagingCost

        [PXUIField(DisplayName = "Packaging Cost")]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
        public Decimal? UsrPackagingCost { get; set; }
        public abstract class usrPackagingCost : PX.Data.BQL.BqlDecimal.Field<usrPackagingCost> { }
        #endregion

        #region UsrLaborCost
        [PXDBDecimal()]
        [PXUIField(DisplayName = "In-house Labor Cost", Visible = false)]
        public Decimal? UsrLaborCost { get; set; }
        public abstract class usrLaborCost : PX.Data.BQL.BqlDecimal.Field<usrLaborCost> { }
        #endregion

        #region UsrHandlingCost
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Handling Cost", Visible = false)]
        public Decimal? UsrHandlingCost { get; set; }
        public abstract class usrHandlingCost : PX.Data.BQL.BqlDecimal.Field<usrHandlingCost> { }
        #endregion

        #region UsrFreightCost
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Freight Cost", Visible = false)]
        public Decimal? UsrFreightCost { get; set; }
        public abstract class usrFreightCost : PX.Data.BQL.BqlDecimal.Field<usrFreightCost> { }
        #endregion

        #region UsrDutyCost
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Duty Cost", Visible = false)]
        public Decimal? UsrDutyCost { get; set; }
        public abstract class usrDutyCost : PX.Data.BQL.BqlDecimal.Field<usrDutyCost> { }
        #endregion

        #region UsrOtherCost
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Other Cost", Visible = false)]
        public Decimal? UsrOtherCost { get; set; }
        public abstract class usrOtherCost : PX.Data.BQL.BqlDecimal.Field<usrOtherCost> { }
        #endregion

        #region UnitCost
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Unit Cost")]
        public Decimal? UsrUnitCost { get; set; }
        public abstract class usrUnitCost : PX.Data.BQL.BqlDecimal.Field<usrUnitCost> { }
        #endregion
    }
}