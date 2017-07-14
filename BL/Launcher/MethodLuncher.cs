using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Web.Script.Serialization;
using IA.IAFG.UN.Contracts.Entities;
using UNWcfTester.ServiceExternal;
using UNWcfTester.ViewModel;

namespace UNWcfTester.BL.Launcher
{
   public class MethodLuncher : Launcher
   {
      private readonly List<MemberInfo> _extServiceMethods;
      private readonly string _methodName;
      private readonly Action _runSecurity;
      private readonly List<ParamCtrlItemModel> _ctrls;
      private readonly string _url;
     
      
      public MethodLuncher(List<MemberInfo> extServiceMethods, string methodname, Action runSecurity, List<ParamCtrlItemModel> ctrls, string url)
         
      {
         this._extServiceMethods = extServiceMethods;
         this._methodName = methodname;
         this._runSecurity = runSecurity;
         this._ctrls = ctrls;
         this._url = url;
      }

      public override object Invoke()
      {
         object methOut = null;
         var classInstance = GetClassInstance();
         // Get method's parameters
         var methInfo = (MethodInfo)(_extServiceMethods.FirstOrDefault(x => x.Name == _methodName));

         if (methInfo != null)
         {
            var parameters = methInfo.GetParameters();
            var paramValues = GetInvokedParams(classInstance, _ctrls).ToArray<object>();

            for (var pii = 0; pii < parameters.Length; pii++)
            {
               if (paramValues[pii] == null) continue;
               var pi = parameters[pii];
               if (string.IsNullOrEmpty(paramValues[pii].ToString()))
               {
                  paramValues[pii] = (pi.ParameterType.IsValueType) ? Activator.CreateInstance(pi.ParameterType) : null;
               }
               if (paramValues[pii] == null) continue;
               if (paramValues[pii].GetType() != pi.ParameterType)
               {
                  paramValues[pii] = Convert.ChangeType(paramValues[pii], pi.ParameterType);
               }
            }
            _runSecurity.Invoke();
            var service = GetProxy(classInstance, _url);
            var result = methInfo.Invoke(classInstance, parameters.Length == 0 ? null : paramValues);

            service.Close();
            methOut = result;
            //var jsonObj = new JavaScriptSerializer();
            //var json = jsonObj.Serialize(result);
            //methOut = JsonHelper.FormatJson(json);
         }

         return methOut;
      }

      private static object GetClassInstance()
      {
         var assembly = Assembly.GetAssembly(typeof(ServiceExternalClient));
         var type = assembly.GetType(typeof(ServiceExternalClient).FullName);
         var classInstance = Activator.CreateInstance(type);
         return classInstance;
      }

      private IEnumerable<object> GetInvokedParams(object classInstance, List<ParamCtrlItemModel> ctrls)
      {

         var paramsval = new List<object>();

         ctrls.ForEach(x => paramsval.Add(GetValue(x)));

         return paramsval;
      }

      private static object GetValue(ParamCtrlItemModel x)
      {
         if (x.IsSearchParam)
         {

            var sp = new SearchParameters();

            foreach (var spv in x.SearchParamValue)
            {
               sp.Parameters.Add(spv.SearchParamKey, spv.SearchParamValue);
            }

            return sp;
         }
         if (x.IsOtherType)
         {
            var serializer = new JavaScriptSerializer();

            var type = x.OtherTypeType;
            //Call generic method with a type resolved in the runtime
            var method = serializer.GetType()
               .GetMethod("Deserialize", new[] { typeof(string) })
               .MakeGenericMethod(new Type[] { type });
            var obj = method.Invoke(serializer, new object[] { x.OtherTypeValue });

            return obj;
         }
         if (x.IsCombobox) return x.ComboboxValue;
         if (x.IsTextbox) return x.TextboxValue;
         if (x.IsDatetime) return x.DatetimeValue;
         return "";
      }


      private ServiceExternalClient GetProxy(object classInstance, string url)
      {

         var service = (ServiceExternalClient)classInstance;
         service.Endpoint.EndpointBehaviors.Add(new ClientSessionBehavior());
         service.Endpoint.Address = new EndpointAddress(url);
         return service;
      }

      
   }
}
