using System.Windows.Documents;
using System.Windows.Media;

namespace UNWcfTester.BL.Launcher
{
   internal class OutputLogger : LauncherDecorator
   {
      private string _selMethod = null;

      public void SetSelMethod(string selMethod)
      {
         _selMethod = selMethod;
      }

      public override object Invoke()
      {
         return GetOuput();
      }
      private FlowDocument GetOuput()
      {
         var _output = new FlowDocument();
         var methodname = string.Empty;
         var par1 = new Paragraph();
         _output.Blocks.Add(par1);

         var par11 = new Paragraph();

         if (_selMethod != null)
         {
            methodname = _selMethod;
            par11.Inlines.Add(new Bold(new Run("Selected method: " + methodname)));
            par11.Foreground = (Brush)Brushes.Blue;

         }
         _output.Blocks.Add(par11);

         var par2 = new Paragraph();
         par2.Inlines.Add(new Bold(new Run("Start request")));
         par2.Foreground = Brushes.CadetBlue;
         _output.Blocks.Add(par2);


         //var classInstance = GetClassInstance();

         

         //// Get method's parameters
         //var methInfo = (MethodInfo)(extServiceMethods.FirstOrDefault(x => x.Name == methodname));

         //if (methInfo != null)
         //{
         //   var parameters = methInfo.GetParameters();
         //   var paramValues = GetInvokedParams(classInstance, ctrls).ToArray<object>();

         //   for (var pii = 0; pii < parameters.Length; pii++)
         //   {
         //      if (paramValues[pii] == null) continue;
         //      var pi = parameters[pii];
         //      if (string.IsNullOrEmpty(paramValues[pii].ToString()))
         //      {
         //         paramValues[pii] = (pi.ParameterType.IsValueType) ? Activator.CreateInstance(pi.ParameterType) : null;
         //      }
         //      if (paramValues[pii] == null) continue;
         //      if (paramValues[pii].GetType() != pi.ParameterType)
         //      {
         //         paramValues[pii] = Convert.ChangeType(paramValues[pii], pi.ParameterType);
         //      }
         //   }
         //   runSecurity.Invoke();
         //   var service = GetProxy(classInstance, url);
         //   var result = methInfo.Invoke(classInstance, parameters.Length == 0 ? null : paramValues);

         //   service.Close();

         //   var jsonObj = new JavaScriptSerializer();
         //   var json = jsonObj.Serialize(result);
         //   _output.Blocks.Add(new Paragraph(new Run((JsonHelper.FormatJson(json)))));
         //}
         string _out = base.Invoke().ToString();
         _output.Blocks.Add(new Paragraph(new Run(_out)));
         var par3 = new Paragraph();
         par3.Inlines.Add(new Bold(new Run("End request")));
         par3.Foreground = (Brush)Brushes.CadetBlue;
         _output.Blocks.Add(par3);

         return _output;
      }

      

      
   }
}
