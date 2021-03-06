﻿using PholioVisualisation.DataAccess;
using PholioVisualisation.PholioObjects;
using System.Collections.Generic;
using System.Linq;
using PholioVisualisation.DataSorting;
using PholioVisualisation.UserData;
using PholioVisualisation.UserData.Repositories;

namespace PholioVisualisation.DataConstruction
{
    public class AreaListProvider
    {
        public virtual IList<IArea> Areas { get; private set; }

        private IAreasReader _areasReader;

        /// <summary>
        /// Parameterless constructor to enable mocking
        /// </summary>
        public AreaListProvider() { }

        public AreaListProvider(IAreasReader areasReader)
        {
            this._areasReader = areasReader;
        }

        public void CreateAreaListFromAreaCodes(IEnumerable<string> areaCodes)
        {
            Areas = areaCodes.Select(areaCode => AreaFactory.NewArea(_areasReader, areaCode)).ToList();
        }

        public void CreateAreaListFromAreaTypeId(int profileId, int areaTypeId, string userId)
        {
            if (AreaListAreaType.IsAreaListAreaType(areaTypeId))
            {
                IAreaListRepository areaListRepository = new AreaListRepository(new fingertips_usersEntities());
                var areaLists = areaListRepository.GetAll(userId);

                IList<Area> areas = new List<Area>();

                foreach (var areaList in areaLists)
                {
                    Area area = new Area()
                    {
                        Code = areaList.PublicId,
                        Name = areaList.ListName,
                        ShortName = areaList.ListName,
                        AreaTypeId = areaList.AreaTypeId
                    };

                    areas.Add(area);
                }

                Areas = areas.Cast<IArea>().ToList();
            }
            else if (areaTypeId > NearestNeighbourAreaType.IdAddition) 
            {
                IList<Area> areas = new List<Area>
                    {
                        new Area{Name = "DUMMY AREA", ShortName = "DUMMY AREA", Code = "DUMMY"}
                    };

                Areas = areas.Cast<IArea>().ToList();
            }
            else if (areaTypeId > CategoryAreaType.IdAddition)
            {
                // e.g All categories for a category type
                int categoryTypeId = CategoryAreaType.GetCategoryTypeIdFromAreaTypeId(areaTypeId);
                Areas = _areasReader.GetCategories(categoryTypeId)
                    .Select(CategoryArea.New).Cast<IArea>().ToList();
            }
            else
            {
                Areas = _areasReader.GetAreasByAreaTypeId(areaTypeId).ToList();
            }
        }

        public void GetAreaListFromAreaTypeIdAndAreaNameSearchText(int areaTypeId, string areaNameSearchText)
        {
            IList<IArea> areas = _areasReader.GetAreasByAreaTypeIdAndAreaNameSearchText(areaTypeId, areaNameSearchText);
            Areas = areas.Cast<IArea>().ToList();
        }

        public void CreateAreaListFromNearestNeighbourAreaCode(string parentAreaCode)
        {
            var nearestNeighbourArea = new NearestNeighbourArea(parentAreaCode);
            var nearestNeighbours = _areasReader.GetNearestNeighbours(nearestNeighbourArea.AreaCodeOfAreaWithNeighbours,
                nearestNeighbourArea.NeighbourTypeId);
            var nearestNeighboursAreas = nearestNeighbours.Select(x => x.NeighbourAreaCode).ToList();
            var areas = _areasReader.GetAreasFromCodes(nearestNeighboursAreas);
            var sortedAreas = new List<IArea>();
            // Sort list of NN by their rank.
            var sequence = 1;
            foreach (var nearestNeighbourAreaCode in nearestNeighboursAreas)
            {
                var area = areas.FirstOrDefault(x => x.Code == nearestNeighbourAreaCode);
                if (area != null)
                {
                    area.Sequence = sequence++;
                }
                sortedAreas.Add(area);
            }
            Areas = sortedAreas;
        }

        public virtual void CreateChildAreaList(string parentAreaCode, int childAreaTypeId)
        {
            Areas = new ChildAreaListBuilder(_areasReader).GetChildAreas(parentAreaCode, childAreaTypeId);
        }

        public virtual void RemoveAreasIgnoredEverywhere(IgnoredAreasFilter ignoredAreasFilter)
        {
            CheckAreasDefined();
            Areas = ignoredAreasFilter.RemoveAreasIgnoredEverywhere(Areas).ToList();
        }

        public virtual void SortByOrderOrName()
        {
            CheckAreasDefined();

            if (Areas.Any() == false) return;

            if (Areas.First().Sequence.HasValue)
            {
                Areas = Areas.OrderBy(x => x.Sequence).ToList();
            }
            else
            {
                Areas = Areas.OrderBy(x => x.Name).ToList();
            }
        }

        private void CheckAreasDefined()
        {
            if (Areas == null)
            {
                throw new FingertipsException("Area list must be created before this can be called");
            }
        }
    }
}