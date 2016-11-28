using System;
using System.IO;
using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Binary; //note supporter in netcore 1.0.1
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;

namespace OnlineMarkerCW.UnitTests.ObjectOperationsHelper

{
    public class ObjectOperations
    {

        private readonly ITestOutputHelper output;

        public ObjectOperations(ITestOutputHelper output)   {this.output = output;}

        //Use it with care, it might be not conistent with it's output, as it ignores the fields which launch expections while searilizing
        public void printObject (object Object) {
            output.WriteLine(
              "Contents of {0} are {1}",
              Object.ToString(),
              JsonConvert.SerializeObject(
                Object,
                Formatting.Indented,
                new JsonSerializerSettings {
                  ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                  ContractResolver = new IgnoreExceptions()
                }
              )
            );
          }

        //Binary Formatter will only be available in future releaset .net core
        // //Defines an object copier
        // //Source http://stackoverflow.com/questions/78536/deep-cloning-objects
        // // Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
        //
        //     public static T Clone<T>(T source)
        //     {
        //         if (!typeof(T).IsSerializable)
        //         {
        //             throw new ArgumentException("The type must be serializable.", "source");
        //         }
        //
        //         // Don't serialize a null object, simply return the default for that object
        //         if (Object.ReferenceEquals(source, null))
        //         {
        //             return default(T);
        //         }
        //
        //         IFormatter formatter = new BinaryFormatter();
        //         Stream stream = new MemoryStream();
        //         using (stream)
        //         {
        //             formatter.Serialize(stream, source);
        //             stream.Seek(0, SeekOrigin.Begin);
        //             return (T)formatter.Deserialize(stream);
        //         }
        //     }

    }

    //This class overrides the default Resolver for the Json Serializer,ignores stream properties and fields that throw exception.
    public class IgnoreExceptions : DefaultContractResolver
      {
          protected override JsonProperty CreateProperty(
              MemberInfo member,
              MemberSerialization memberSerialization
          )
          {
              JsonProperty property = base.CreateProperty(member, memberSerialization);
              // if (typeof(Stream).IsAssignableFrom(property.PropertyType))
              //   {
              //       property.Ignored = true;
              //   }

            property.ShouldSerialize = instance =>
              {
                   try
                   {
                       PropertyInfo prop = (PropertyInfo)member;
                       if (prop.CanRead)
                       {
                           prop.GetValue(instance, null);
                           return true;
                       }
                   }
                   catch
                   {
                   }
                   return false;
              };


              return property;
          }
      }

}
