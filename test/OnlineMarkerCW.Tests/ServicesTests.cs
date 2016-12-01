using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft​.AspNetCore​.Http​.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineMarkerCW.Models;
using System.Threading.Tasks;
using OnlineMarkerCW.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using OnlineMarkerCW.Interfaces;
using OnlineMarkerCW.ViewModels;
using OnlineMarkerCW.Services;
using  OnlineMarkerCW.Data;
using OnlineMarkerCW.UnitTests.ObjectOperationsHelper;
using System.Linq;

 namespace OnlineMarkerCW.UnitTests.Services
 {
     public class ServicesController_UnitTests
     {
        private readonly DbServices _DbServices;
        private readonly LoggerFactory _logger;
        private readonly ITestOutputHelper output;
        private ObjectOperations objectOperations;
        private Mock<ApplicationDbContext> m_dbContext;
        private Work testWork; //create a default test work and then ovewrite it if necessary
        private List<Work> sampleList;
        private IQueryable<Work> workList;
        private ApplicationUser testUserOwner;

          public ServicesController_UnitTests(ITestOutputHelper output)
          {
              this.objectOperations        = new ObjectOperations (output);
              this.output                  = output;
              _logger                      = new LoggerFactory();
              this.testWork                = new Work() {WorkID = 1, FilePath="path" , Mark = 10, Marked = true, Feedback = "feedback", SubmitDate =  DateTime.Now, MarkDate =  DateTime.Now, Owner = new ApplicationUser(), Marker = new ApplicationUser()};
              this.m_dbContext            = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

              //pass mock db context to the services
              _DbServices                 = new DbServices(m_dbContext.Object);

              //create mock list for testing the Db queries
              this.testUserOwner = new ApplicationUser() {UserName = "owner@test.com"};
              this.sampleList = new List<Work>{
                new Work() {WorkID = 2, FilePath="path2" ,  Marked = false, SubmitDate =  DateTime.Now.AddDays(5), Owner = testUserOwner},
                new Work() {WorkID = 1, FilePath="path1" ,  Marked = false, SubmitDate =  DateTime.Now, Owner = new ApplicationUser()},
                new Work() {WorkID = 7, FilePath="path7" ,  Marked = false, SubmitDate =  DateTime.Now, Owner = testUserOwner},
                new Work() {WorkID = 4, FilePath="path4" ,  Marked = false, SubmitDate =  DateTime.Now, Owner = new ApplicationUser()},
                new Work() {WorkID = 8, FilePath="path8" ,  Marked = false, SubmitDate =  DateTime.Now.AddDays(1), Owner = testUserOwner},
                new Work() {WorkID = 6, FilePath="path6" ,  Marked = false, SubmitDate =  DateTime.Now, Owner = new ApplicationUser()},
                new Work() {WorkID = 3, FilePath="path3" ,  Marked = false, SubmitDate =  DateTime.Now.AddDays(3), Owner = new ApplicationUser()},
                new Work() {WorkID = 5, FilePath="path5" ,  Marked = false, SubmitDate =  DateTime.Now, Owner = new ApplicationUser()}
              };
              this.workList = sampleList.AsQueryable();


          }
//***************************************//
//**START Testing non-query Db services**//
//*************************************//
         [Fact]
         public void AddWork_Adds_a_Work_via_context()
         {
          //Accrange
           var mockSet = new Mock<DbSet<Work>>();
           m_dbContext.Setup(m => m.Works).Returns(mockSet.Object);

           //Act
           _DbServices.AddWork(testWork);

           //Verify Using mock
           mockSet.Verify(w => w.Add(It.IsAny<Work>()), Times.Once());
           m_dbContext.Verify(m => m.SaveChanges(), Times.Once());
         }

         [Fact]
         public void RemoveWork_Removes_a_Work_via_context()
         {
          //Accrange
           var mockSet = new Mock<DbSet<Work>>();
           m_dbContext.Setup(m => m.Works).Returns(mockSet.Object);

           //Act
           _DbServices.RemoveWork(testWork);

           //Verify Using mock
           mockSet.Verify(w => w.Remove(It.IsAny<Work>()), Times.Once());
           m_dbContext.Verify(m => m.SaveChanges(), Times.Once());
         }

         [Fact]
         public void MarkWork_Updates_a_Work_via_context()
         {
           var mockSet = new Mock<DbSet<Work>>();
           m_dbContext.Setup(m => m.Works).Returns(mockSet.Object);

           //Act
           _DbServices.MarkWork(testWork, new ApplicationUser(), "feedback", 50);

           //Verify Using mock
           mockSet.Verify(w => w.Update(It.IsAny<Work>()), Times.Once());
           m_dbContext.Verify(m => m.SaveChanges(), Times.Once());
         }
//*************************************//
//**END Testing non-query Db services**//
//************************************//

//*************************************//
//**Start Testing query Db services**//
//************************************//
        [Fact]
        public async Task GetSubmitedWorks_GetByUser_and_OrderByDate()
        {
         //Arrange
          // mock the test resuls -- pick ones by the testUserOwner and sort them by date in acesding order
          var testWorks = new List<Work>  {sampleList[2], sampleList[4],sampleList[0]};
          //--Mock the DbSet and make is queryable
          var mockSet = new Mock<DbSet<Work>>();

          mockSet.As<IAsyncEnumerable<Work>>()
              .Setup(m => m.GetEnumerator())
              .Returns(new TestAsyncEnumerator<Work>(workList.GetEnumerator()));

          mockSet.As<IQueryable<Work>>()
              .Setup(m => m.Provider)
              .Returns(new TestAsyncQueryProvider<Work>(workList.Provider));

          mockSet.As<IQueryable<Work>>().Setup(m => m.Expression).Returns(workList.Expression);
          mockSet.As<IQueryable<Work>>().Setup(m => m.ElementType).Returns(workList.ElementType);
          mockSet.As<IQueryable<Work>>().Setup(m => m.GetEnumerator()).Returns(() => workList.GetEnumerator());


          m_dbContext.Setup(m => m.Works).Returns(mockSet.Object);

          //Act
          var resultWorks = await _DbServices.GetSubmitedWorks(testUserOwner);
          output.WriteLine("#################################");
          output.WriteLine("Printing the result");
          output.WriteLine("#################################");
          objectOperations.printObject(resultWorks);

          //Verify Using mock
          Assert.Equal(testWorks, resultWorks);
        }

        [Fact]
        public async Task GetWorkWithID_GetsWorkByID()
        {
         //Arrange
          //--Mock the DbSet and make is queryable
          var mockSet = new Mock<DbSet<Work>>();

          mockSet.As<IAsyncEnumerable<Work>>()
              .Setup(m => m.GetEnumerator())
              .Returns(new TestAsyncEnumerator<Work>(workList.GetEnumerator()));

          mockSet.As<IQueryable<Work>>()
              .Setup(m => m.Provider)
              .Returns(new TestAsyncQueryProvider<Work>(workList.Provider));

          mockSet.As<IQueryable<Work>>().Setup(m => m.Expression).Returns(workList.Expression);
          mockSet.As<IQueryable<Work>>().Setup(m => m.ElementType).Returns(workList.ElementType);
          mockSet.As<IQueryable<Work>>().Setup(m => m.GetEnumerator()).Returns(() => workList.GetEnumerator());


          m_dbContext.Setup(m => m.Works).Returns(mockSet.Object);

          //Act
          var resultWorks1 = await _DbServices.GetWorkWithID(2);
          var resultWorks2 = await _DbServices.GetWorkWithID(4);
          var resultWorks3 = await _DbServices.GetWorkWithID(6);
          output.WriteLine("#################################");
          output.WriteLine("Printing the result");
          output.WriteLine("#################################");
          objectOperations.printObject(resultWorks1);
          objectOperations.printObject(resultWorks2);
          objectOperations.printObject(resultWorks3);

          //Verify Using mock
          Assert.Equal(sampleList[0], resultWorks1);
          Assert.Equal(sampleList[3], resultWorks2);
          Assert.Equal(sampleList[5], resultWorks3);
        }



        //This test invokes the include operator, which cannot be called seperatly from the EF before entitycore 1.0.1, which required changing the dependencies, which may break the app.
        //https://github.com/aspnet/EntityFramework/issues/5735
        // [Fact]
        // public async Task GetWorksAndOwners_Returns_the_whole_DataSet_Sorted_ByDate()
        // {
        //  //Arrange
        //   // mock the test resuls -- pick ones by the testUserOwner and sort them by date in acesding order
        //   var testWorks = new List<Work>  {sampleList[1], sampleList[2],sampleList[3],sampleList[5],sampleList[7],sampleList[4],sampleList[6],sampleList[0]};
        //   //--Mock the DbSet and make is queryable
        //   var mockSet = new Mock<DbSet<Work>>();
        //
        //   mockSet.As<IAsyncEnumerable<Work>>()
        //       .Setup(m => m.GetEnumerator())
        //       .Returns(new TestAsyncEnumerator<Work>(workList.GetEnumerator()));
        //
        //   mockSet.As<IQueryable<Work>>()
        //       .Setup(m => m.Provider)
        //       .Returns(new TestAsyncQueryProvider<Work>(workList.Provider));
        //
        //   mockSet.As<IQueryable<Work>>().Setup(m => m.Expression).Returns(workList.Expression);
        //   mockSet.As<IQueryable<Work>>().Setup(m => m.ElementType).Returns(workList.ElementType);
        //   mockSet.As<IQueryable<Work>>().Setup(m => m.GetEnumerator()).Returns(() => workList.GetEnumerator());
        //
        //
        //   m_dbContext.Setup(m => m.Works).Returns(mockSet.Object);
        //
        //   //Act
        //   var resultWorks = await _DbServices.GetWorksAndOwners();
        //   output.WriteLine("#################################");
        //   output.WriteLine("Printing the result");
        //   output.WriteLine("#################################");
        //   objectOperations.printObject(resultWorks);
        //
        //   //Verify Using mock
        //   Assert.Equal(testWorks, resultWorks);
        // }


     }
 }
