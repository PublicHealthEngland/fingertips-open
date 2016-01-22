﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PholioVisualisation.PholioObjects;

namespace PholioObjectsTest
{
     [TestClass]
     public class WhenUsingGroupRootSummary
     {

         [TestInitialize]
         public void Init()
         {
         }
         
         [TestMethod]
         public void CompareTo_Returns_Collection_SortedBy_GenderSequence()
         {

             // Arrange
             var summaries = new List<GroupRootSummary>()
             {
                new GroupRootSummary()
                {
                 IndicatorName = "Test Indicator"  ,
                 IndicatorId = 1,
                 SexId = 2,
                 AgeId=10,
                 GroupId = 10
                } ,

                new GroupRootSummary()
                {
                 IndicatorName = "Test Indicator"  ,
                 IndicatorId = 2,
                 SexId = 4,
                 AgeId=5,
                 GroupId = 10
                } ,

                new GroupRootSummary()
                {
                 IndicatorName = "Test Indicator"  ,
                 IndicatorId = 3,
                 SexId = 1,
                 AgeId=5,
                 GroupId = 10
                } ,
            
             }.ToList();
            
              // Act
             summaries.Sort();

             // Assert
             // should be sorted by Name then Sex Sequence (person, Male and Female Order)
             Assert.IsTrue(summaries[0].IndicatorId == 2 && summaries[1].IndicatorId == 3);
         }

         [TestMethod]
         public void CompareTo_Returns_Collection_SortedBy_Name()
         {

             // Arrange
             var summaries = new List<GroupRootSummary>()
             {
                new GroupRootSummary()
                {
                 IndicatorName = "Test b"  ,
                 IndicatorId = 1,
                 SexId = 2,
                 AgeId=10,
                 GroupId = 10
                } ,

                new GroupRootSummary()
                {
                 IndicatorName = "Test a"  ,
                 IndicatorId = 2,
                 SexId = 4,
                 AgeId=5,
                 GroupId = 10
                } ,

                new GroupRootSummary()
                {
                 IndicatorName = "Test c"  ,
                 IndicatorId = 3,
                 SexId = 1,
                 AgeId=5,
                 GroupId = 10
                } ,
                new GroupRootSummary()
                {
                 IndicatorName = "Test b"  ,
                 IndicatorId = 4,
                 SexId = 4,
                 AgeId=10,
                 GroupId = 10
                } ,
            
             }.ToList();

             // Act
             summaries.Sort();

             // Assert
             Assert.IsTrue(summaries[0].IndicatorId == 2 && 
                 summaries[1].IndicatorId == 4 && 
                 summaries[2].IndicatorId== 1 &&
                 summaries[3].IndicatorId ==3
                 );
         }
     
     }
}