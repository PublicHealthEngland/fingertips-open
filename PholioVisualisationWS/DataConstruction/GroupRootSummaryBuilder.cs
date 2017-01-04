﻿using System.Collections.Generic;
using System.Linq;
using PholioVisualisation.DataAccess;
using PholioVisualisation.PholioObjects;

namespace PholioVisualisation.DataConstruction
{
    public class GroupRootSummaryBuilder
    {
        public IList<GroupRootSummary> Build(int profileId, int areaTypeId)
        {
            IndicatorMetadataProvider metadataRepo = IndicatorMetadataProvider.Instance;
            IList<int> groupIds = ReaderFactory.GetGroupDataReader().GetGroupingIds(profileId);

            IList<GroupRootSummary> groupRootSummaries = new List<GroupRootSummary>();
            foreach (int groupId in groupIds)
            {
                IList<Grouping> groupings =
                    ReaderFactory.GetGroupDataReader()
                        .GetGroupingsByGroupIdAndAreaTypeIdOrderedBySequence(groupId, areaTypeId);
                GroupRootBuilder rootBuilder = new GroupRootBuilder();
                IList<GroupRoot> roots = rootBuilder.BuildGroupRoots(groupings);

                IndicatorMetadataCollection metadataCollection = metadataRepo.GetIndicatorMetadataCollection(groupings);
                foreach (var groupRoot in roots)
                {
                    IndicatorMetadata indicatorMetadata =
                        metadataCollection.GetIndicatorMetadataById(groupRoot.IndicatorId);

                    var duplicate = groupRootSummaries.FirstOrDefault(
                        x =>
                            x.Age.Id == groupRoot.AgeId &&
                            x.Sex.Id == groupRoot.SexId &&
                            x.IndicatorId == groupRoot.IndicatorId);

                    if (duplicate == null)
                    {
                        groupRootSummaries.Add(
                            new GroupRootSummary
                            {
                                GroupId = groupRoot.FirstGrouping.GroupId,
                                Age = groupRoot.Age,
                                Sex = groupRoot.Sex,
                                IndicatorId = groupRoot.IndicatorId,
                                IndicatorName = indicatorMetadata.Descriptive[IndicatorMetadataTextColumnNames.Name],
                                StateSex = groupRoot.StateSex,
                                StateAge = groupRoot.StateAge,
                                IndicatorUnit = indicatorMetadata.Unit
                            });
                    }
                }
            }
            return groupRootSummaries;
        }
    }
}