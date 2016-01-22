﻿using System.Collections.Generic;
using System.Linq;
using Fpm.ProfileData.Entities.Profile;

namespace Fpm.ProfileData
{
    public class AllowedData
    {
        private List<int> ageIds;
        private List<string> areaCodes;
        private ProfilesReader profilesReader;
        private List<int> sexIds;
        private List<int> valueNoteIds;
        private List<Category> categories;

        public AllowedData(ProfilesReader profilesReader)
        {
            this.profilesReader = profilesReader;
        }

        public List<int> AgeIds
        {
            get
            {
                if (ageIds == null)
                {
                    ageIds = profilesReader.GetAllAgeIds().ToList();
                }
                return ageIds;
            }
        }

        public List<int> SexIds
        {
            get
            {
                if (sexIds == null)
                {
                    sexIds = profilesReader.GetAllSexIds().ToList();
                }
                return sexIds;
            }
        }

        public List<string> AreaCodes
        {
            get
            {
                if (areaCodes == null)
                {
                    areaCodes = profilesReader.GetAllAreaCodes().ToList();
                }
                return areaCodes;
            }
        }

        public List<int> ValueNoteIds
        {
            get
            {
                if (valueNoteIds == null)
                {
                    valueNoteIds = profilesReader.GetAllValueNoteIds().ToList();
                }
                return valueNoteIds;
            }
        }

        public List<Category> Categories
        {
            get
            {
                if (categories == null)
                {
                    categories = profilesReader.GetAllCategories().ToList();
                }
                return categories;
            }
        }
    }
}