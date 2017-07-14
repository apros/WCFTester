using System.Web.Script.Serialization;

namespace UNWcfTester.BL.Launcher
{
   public class JsonLauncherDecorator : LauncherDecorator{
      
      public override object Invoke()
      {
         var jsonObj = new JavaScriptSerializer();
         var obj = base.Invoke();
         var json = jsonObj.Serialize(obj);
         return JsonHelper.FormatJson(json);
      }
   }
}
