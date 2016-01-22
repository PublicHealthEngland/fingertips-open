﻿using PholioVisualisation.PholioObjects;

namespace PholioVisualisation.DataAccess
{
    public class ProfileInitialiser
    {
        private ProfileConfig profileConfig;

        public ProfileInitialiser(ProfileConfig profileConfig)
        {
            this.profileConfig = profileConfig;
        }

        public Profile InitialisedProfile
        {
            get
            {
                if (profileConfig == null)
                {
                    return null;
                }

                var groupIds = ReaderFactory.GetGroupDataReader().GetGroupingIds(profileConfig.ProfileId);
                return new Profile(groupIds)
                {
                    Id = profileConfig.ProfileId,
                    Name = profileConfig.Name,
                    UrlKey = profileConfig.UrlKey,
                    IsNational = profileConfig.IsNational
                };
            }
        }

    }
}