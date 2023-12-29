﻿using ASCJewelryLibrary.INKit.CacheExt;
using ASCJewelryLibrary.INKit.Interfaces;
using PX.Data;
using PX.Objects.IN;
using static ASCJewelryLibrary.Common.Descriptor.ASCJConstants;

namespace ASCJewelryLibrary.INKit.Descriptor
{
    /// <summary>
    /// Custom attribute that subscribes to row events (inserted, updated, and deleted) for INKitSpecNonStkDet and INKitSpecStkDet DACs. 
    /// The class processes these events, updating the cost rollup in the associated INKitSpecHdr DAC extension based on the row's ASCJCostRollupType value.
    /// </summary>
    public class ASCJCostAssignmentAttribute : PXEventSubscriberAttribute, IPXRowUpdatedSubscriber, IPXRowInsertedSubscriber, IPXRowDeletedSubscriber
    {
        #region Events
        public void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            var hdrCache = sender.Graph.Caches[typeof(INKitSpecHdr)];
            if (e.Row is INKitSpecNonStkDet nonStkDet)
            {
                ProcessRowInserted<INKitSpecNonStkDet, ASCJINKitSpecNonStkDetExt>(hdrCache, nonStkDet);
            }
            else if (e.Row is INKitSpecStkDet stkDet)
            {
                ProcessRowInserted<INKitSpecStkDet, ASCJINKitSpecStkDetExt>(hdrCache, stkDet);
            }
        }

        public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var hdrCache = sender.Graph.Caches[typeof(INKitSpecHdr)];
            if (e.Row is INKitSpecNonStkDet nonStkDet)
            {
                var oldRow = e.OldRow as INKitSpecNonStkDet;
                ProcessRowUpdated<INKitSpecNonStkDet, ASCJINKitSpecNonStkDetExt>(hdrCache, nonStkDet, oldRow);
            }
            else if (e.Row is INKitSpecStkDet stkDet)
            {
                var oldRow = e.OldRow as INKitSpecStkDet;
                ProcessRowUpdated<INKitSpecStkDet, ASCJINKitSpecStkDetExt>(hdrCache, stkDet, oldRow);
            }
        }

        public void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            var hdrCache = sender.Graph.Caches[typeof(INKitSpecHdr)];
            if (e.Row is INKitSpecNonStkDet nonStkDet)
            {
                ProcessRowDeleted<INKitSpecNonStkDet, ASCJINKitSpecNonStkDetExt>(hdrCache, nonStkDet);
            }
            else if (e.Row is INKitSpecStkDet stkDet)
            {
                ProcessRowDeleted<INKitSpecStkDet, ASCJINKitSpecStkDetExt>(hdrCache, stkDet);
            }
        }
        #endregion

        #region ServiceMethods
        /// <summary>
        /// Processes the row inserted event for a specific DAC and its extension, updating the cost rollup.
        /// </summary>
        /// <typeparam name="TDac">The DAC type of the row being inserted.</typeparam>
        /// <typeparam name="TDacExt">The DAC extension type that implements the IASCJCostRollup interface.</typeparam>
        /// <param name="hdrCache">The cache of the INKitSpecHdr DAC.</param>
        /// <param name="row">The inserted row of type TDac.</param>
        public static void ProcessRowInserted<TDac, TDacExt>(PXCache hdrCache, TDac row)
            where TDac : class, IBqlTable, new()
            where TDacExt : PXCacheExtension<TDac>, IASCJCostRollup
        {
            var rowExt = PXCache<TDac>.GetExtension<TDacExt>(row);
            CostAddition(hdrCache, (INKitSpecHdr)hdrCache.Current, rowExt.UsrASCJCostRollupType, rowExt.UsrASCJExtCost);
        }

        /// <summary>
        /// Processes the row updated event for a specific DAC and its extension, updating the cost rollup.
        /// </summary>
        /// <typeparam name="TDac">The DAC type of the row being updated.</typeparam>
        /// <typeparam name="TDacExt">The DAC extension type that implements the IASCJCostRollup interface.</typeparam>
        /// <param name="hdrCache">The cache of the INKitSpecHdr DAC.</param>
        /// <param name="row">The updated row of type TDac.</param>
        /// <param name="oldRow">The original row of type TDac before the update.</param>
        public static void ProcessRowUpdated<TDac, TDacExt>(PXCache hdrCache, TDac row, TDac oldRow)
            where TDac : class, IBqlTable, new()
            where TDacExt : PXCacheExtension<TDac>, IASCJCostRollup
        {
            var oldRowExt = PXCache<TDac>.GetExtension<TDacExt>(oldRow);
            var rowExt = PXCache<TDac>.GetExtension<TDacExt>(row);

            if (oldRowExt.UsrASCJCostRollupType == rowExt.UsrASCJCostRollupType)
            {
                CostDeduction(hdrCache, (INKitSpecHdr)hdrCache.Current, rowExt.UsrASCJCostRollupType, oldRowExt.UsrASCJExtCost);
                CostAddition(hdrCache, (INKitSpecHdr)hdrCache.Current, rowExt.UsrASCJCostRollupType, rowExt.UsrASCJExtCost);
            }
            else
            {
                CostDeduction(hdrCache, (INKitSpecHdr)hdrCache.Current, oldRowExt.UsrASCJCostRollupType, rowExt.UsrASCJExtCost);
                CostAddition(hdrCache, (INKitSpecHdr)hdrCache.Current, rowExt.UsrASCJCostRollupType, rowExt.UsrASCJExtCost);
            }
        }

        /// <summary>
        /// Processes the row deleted event for a specific DAC and its extension, updating the cost rollup.
        /// </summary>
        /// <typeparam name="TDac">The DAC type of the row being deleted.</typeparam>
        /// <typeparam name="TDacExt">The DAC extension type that implements the IASCJCostRollup interface.</typeparam>
        /// <param name="hdrCache">The cache of the INKitSpecHdr DAC.</param>
        /// <param name="row">The deleted row of type TDac.</param>
        private void ProcessRowDeleted<TDac, TDacExt>(PXCache hdrCache, TDac row)
            where TDac : class, IBqlTable, new()
            where TDacExt : PXCacheExtension<TDac>, IASCJCostRollup
        {
            var rowExt = PXCache<TDac>.GetExtension<TDacExt>(row);
            CostDeduction(hdrCache, (INKitSpecHdr)hdrCache.Current, rowExt.UsrASCJCostRollupType, rowExt.UsrASCJExtCost);
        }

        ///<summary>
        ///Adds the specified cost value to the cost field of the INKitSpecHdr extension record.
        ///</summary>
        /// <param name="cache">The cache of the record</param>
        /// <param name="currentRow">The current record in the cache</param>
        /// <param name="rollupType">The type of cost being added (e.g. Fabrication, Packaging, etc.)</param>
        /// <param name="value">The value being added to the cost field</param>
        public static void CostAddition(PXCache cache, INKitSpecHdr currentRow, string rollupType, decimal? value)
        {
            if (currentRow == null) return;
            var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(currentRow);
            switch (rollupType)
            {
                case CostRollupType.PreciousMetal:
                    {
                        var result = rowExt.UsrASCJPreciousMetalCost + value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJPreciousMetalCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.Fabrication:
                    {
                        var result = rowExt.UsrASCJFabricationCost + value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJFabricationCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.Packaging:
                    {
                        var result = rowExt.UsrASCJPackagingCost + value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJPackagingCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.PackagingForLabor:
                    {
                        var result = rowExt.UsrASCJPackagingLaborCost + value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJPackagingLaborCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.Labor:
                    {
                        var result = rowExt.UsrASCJLaborCost + value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJLaborCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.Materials:
                    {
                        var result = rowExt.UsrASCJOtherMaterialsCost + value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJOtherMaterialsCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.Freight:
                    {
                        var result = rowExt.UsrASCJFreightCost + value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJFreightCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.Handling:
                    {
                        var result = rowExt.UsrASCJHandlingCost + value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJHandlingCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.Duty:
                    {
                        var result = rowExt.UsrASCJDutyCost + value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJDutyCost>(currentRow, result);
                    }
                    break;
                default:
                    break;
            }
        }
        ///<summary>
        /// Method to update cost of different components of a kit, by deducting a specific value.
        ///</summary>
        ///<param name="cache">Cache object used for updating the cache of the selected item</param>
        ///<param name="currentRow">The current item being processed</param>
        ///<param name="rollupType">The type of cost to be deducted from the kit</param>
        ///<param name="value">The value to be deducted from the kit cost</param>
        public static void CostDeduction(PXCache cache, INKitSpecHdr currentRow, string rollupType, decimal? value)
        {
            if (currentRow == null) return;
            var rowExt = PXCache<INKitSpecHdr>.GetExtension<ASCJINKitSpecHdrExt>(currentRow);
            switch (rollupType)
            {
                case CostRollupType.PreciousMetal:
                    {
                        var result = rowExt.UsrASCJPreciousMetalCost - value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJPreciousMetalCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.Fabrication:
                    {
                        var result = rowExt.UsrASCJFabricationCost - value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJFabricationCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.Packaging:
                    {
                        var result = rowExt.UsrASCJPackagingCost - value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJPackagingCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.PackagingForLabor:
                    {
                        var result = rowExt.UsrASCJPackagingLaborCost - value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJPackagingLaborCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.Labor:
                    {
                        var result = rowExt.UsrASCJLaborCost - value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJLaborCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.Materials:
                    {
                        var result = rowExt.UsrASCJOtherMaterialsCost - value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJOtherMaterialsCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.Freight:
                    {
                        var result = rowExt.UsrASCJFreightCost - value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJFreightCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.Handling:
                    {
                        var result = rowExt.UsrASCJHandlingCost - value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJHandlingCost>(currentRow, result);
                    }
                    break;
                case CostRollupType.Duty:
                    {
                        var result = rowExt.UsrASCJDutyCost - value;
                        cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJDutyCost>(currentRow, result);
                    }
                    break;
                default:
                    break;
            }
        }
        public static void CostValueZeroing(PXCache cache, INKitSpecHdr currentRow, string rollupType)
        {
            switch (rollupType)
            {
                case CostRollupType.PreciousMetal:
                    cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJPreciousMetalCost>(currentRow, 0m);
                    break;
                case CostRollupType.Fabrication:
                    cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJFabricationCost>(currentRow, 0m);

                    break;
                case CostRollupType.Packaging:
                    cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJPackagingCost>(currentRow, 0m);

                    break;
                case CostRollupType.PackagingForLabor:
                    cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJPackagingLaborCost>(currentRow, 0m);
                    break;
                case CostRollupType.Labor:
                    cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJLaborCost>(currentRow, 0m);

                    break;
                case CostRollupType.Materials:
                    cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJOtherMaterialsCost>(currentRow, 0m);

                    break;
                case CostRollupType.Freight:
                    cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJFreightCost>(currentRow, 0m);

                    break;
                case CostRollupType.Handling:
                    cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJHandlingCost>(currentRow, 0m);

                    break;
                case CostRollupType.Duty:
                    cache.SetValueExt<ASCJINKitSpecHdrExt.usrASCJDutyCost>(currentRow, 0m);
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}