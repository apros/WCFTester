using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using IA.IAFG.UN.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Web.Script.Serialization;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;
using UNWcfTester.BL;
using UNWcfTester.BL.Launcher;
using UNWcfTester.ServiceExternal;

namespace UNWcfTester.ViewModel
{
   /// <summary>
   /// This class contains properties that the main View can data bind to.
   /// <para>
   /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
   /// </para>
   /// <para>
   /// You can also use Blend to data bind with the tool's support.
   /// </para>
   /// <para>
   /// See http://www.galasoft.ch/mvvm
   /// </para>
   /// </summary>
   public class MainViewModel : ViewModelBase
   {
      #region Private Members
      private Endpoint _selectedEndpoint;
      private string _selectedMethod;
      //string _uri;
      List<MemberInfo> _externalServiceMethods;
      ObservableCollection<string> _methodNames;
      FlowDocument _output;
      public new event PropertyChangedEventHandler PropertyChanged;
      ObservableCollection<ParamCtrlItemModel> _paramItems;
      int _newParamCounter;
      ObservableCollection<string> _methodTemplates;
      string _selectedTemplate;
      #endregion

      #region Constructor
      /// <summary>
      /// Initializes a new instance of the MainViewModel class.
      /// </summary>
      public MainViewModel()
      {
         SetEndpoints();

         SetMethodList();

         SetOutput();

         SetMethodParameters();
      }

      private void SetMethodParameters()
      {
         _paramItems = new ObservableCollection<ParamCtrlItemModel>();
      }

      private void SetOutput()
      {
         _output = new FlowDocument();
      }

      private void SetEndpoints()
      {
         FillEndpoints();

         SelectDefaultEndpoint();
      }

      #endregion

      #region Methods
      private void SelectDefaultEndpoint()
      {
         if (Endpoints.ToList().Count > 0)
         {
            SelectedEndpoint = Endpoints.First();
         }
      }



      private void FillEndpoints()
      {
         Endpoints = new ObservableCollection<Endpoint>(){
               new Endpoint(){Name="Local", Uri= ConfigurationManager.AppSettings["Local"]},
               new Endpoint(){Name="ASMB", Uri= ConfigurationManager.AppSettings["ASMB"]},
               new Endpoint(){Name="FNCT", Uri=ConfigurationManager.AppSettings["FNCT"]},
               new Endpoint(){Name="INTG", Uri= ConfigurationManager.AppSettings["INTG"]},
               new Endpoint(){Name="ACCP", Uri=ConfigurationManager.AppSettings["ACCP"]}
         };
      }

      private void RunAuthentication()
      {
         if (SelectedEndpoint == null) throw new ApplicationException("Endpoint not defined");
         var sec = new ServiceExternal.ServiceExternalClient();
         sec.Endpoint.EndpointBehaviors.Add(new ClientSessionBehavior());
         sec.Endpoint.Address = new EndpointAddress(SelectedEndpoint.Uri);
         ClientSessionBehavior.ApplicationProperties = sec.GetCurrentUserAccess();
         sec.Close();
      }

      private void InvokeMethod()
      {

       
         var listBox = (ListBox) PnlPropertyContainer.Children[0];
         var paramCtrls = (listBox.Items.Cast<ParamCtrlItemModel>()).ToList();
         var methLauncher = new MethodLuncher(_externalServiceMethods, this.SelectedMethod, RunAuthentication, paramCtrls,
            SelectedEndpoint.Uri);
         var jsonDecorator = new JsonLauncherDecorator();
         jsonDecorator.SetLauncher(methLauncher);
         var outputLogger = new OutputLogger();
         outputLogger.SetLauncher(jsonDecorator);
         outputLogger.SetSelMethod(SelectedMethod);
         RichMessage.Document = (FlowDocument)outputLogger.Invoke();
      }

      private void SetMethodList()
      {
         _externalServiceMethods = GetExternalServiceMethods();

         var methodNames = _externalServiceMethods
             .Where(x => !x.Name.Contains("Param"))
             .Select(y => y.Name)
             .Distinct()
             .ToList();

         methodNames.Sort();
         MethodNames = methodNames.ToObservableCollection<string>();
      }

      private static List<MemberInfo> GetExternalServiceMethods()
      {
         // Get the current executing assembly and return all exported types included in it:
         var exportedTypes = Assembly.GetExecutingAssembly().GetExportedTypes();

         // The list to store the method names
         var methodsList = new List<MemberInfo>();

         foreach (var exportedType in exportedTypes)
         {
            foreach (var implementedInterfaces in exportedType.GetInterfaces())
            {
               var mi = implementedInterfaces.GetMembers();
               foreach (var member in mi)
               {
                  if (member.MemberType != MemberTypes.Method) continue;
                  // Check the method attributes and make sure that it is marked as "OperationContract":
                  var methodAttributes = member.GetCustomAttributes(false);
                  if (methodAttributes.Length > 0 && methodAttributes.Any(p => p is OperationContractAttribute))
                  {
                     methodsList.Add(member);
                  }
               };
            };

         };

         return methodsList;
      }

      private void CreateControls(IEnumerable<ParameterInfo> paramsInfo)
      {

         _paramItems.Clear();

         foreach (var paramInfo in paramsInfo)
         {
            var pcim = new ParamCtrlItemModel()
            {
               IsCheckbox = false
               , IsCombobox = false
               , IsDatetime = false
               , IsTextbox = false
               , IsSearchParam = false
               , Label = paramInfo.Name
               , TextboxValue = GetDefaultValue(paramInfo.ParameterType) == null ? string.Empty : GetDefaultValue(paramInfo.ParameterType).ToString()
               , CheckboxValue = default(bool)
               , ComboboxValue = default(bool)
               , DatetimeValue = DateTime.Now
               , SearchParamValue = new ObservableCollection<SearchParameterItem>()
            };

            switch (paramInfo.ParameterType.Name)
            {
               case "Int32":
               case "Int16":
               case "Double":
               case "Single":
               case "float":
               case "Decimal":
               case "Nullable`1":
               case "Guid":
               case "String":
                  pcim.IsTextbox = true;
                  break;
               case "Boolean":
                  pcim.IsCheckbox = true;
                  break;
               case "DateTime":
                  pcim.IsDatetime = true;
                  break;
               case "SearchParameters":
                  pcim.IsSearchParam = true;
                  var sp = new SearchParameters {Parameters = new Dictionary<string, string> {{" ", " "}}};
                  foreach (var kv in sp.Parameters)
                  {
                     pcim.SearchParamValue.Add(new SearchParameterItem() { SearchParamKey = kv.Key, SearchParamValue = kv.Value });
                  }
                  break;
               default:
                  pcim.IsOtherType = true;
                  pcim.OtherTypeType = paramInfo.ParameterType;
                  var assembly = Assembly.GetAssembly(paramInfo.ParameterType);
                  var _type = assembly.GetType(paramInfo.ParameterType.FullName);
                  var obj = Activator.CreateInstance(_type);
                  var json = new JavaScriptSerializer().Serialize(obj);
                  pcim.OtherTypeValue = JsonHelper.FormatJson(json);
                  break;
            }
            _paramItems.Add(pcim);
         }
      }

      private static object GetDefaultValue(Type t)
      {
         return t.IsValueType ? Activator.CreateInstance(t) : null;
      }

      private void WcfMethodsChanged()
      {
         if (SelectedMethod == null) return;

         var methodname = SelectedMethod;
         if (ExternalServiceMethods == null) return;

         var methInfo = (MethodInfo)(ExternalServiceMethods.FirstOrDefault(x => x.Name == methodname));

         if (methInfo != null)
         {
            var pars = methInfo.GetParameters();
            CreateControls(pars);
         }
         var templatePath = GetDefaultTemplatePath(methInfo.Name);

         // fill templates in UI
         MethodTemplates = GetTemplatesForMethod(methInfo.Name)
            .Select(Path.GetFileNameWithoutExtension)
            .ToObservableCollection();




         if (!string.IsNullOrEmpty(templatePath))
         {
            // select default template in UI
            SelectedTemplate = Path.GetFileNameWithoutExtension(templatePath);
         }

      }

      private string GetDefaultTemplatePath(string methName)
      {
         // find default template         
         var methFiles = GetTemplatesForMethod(methName);
         var methodFiles = methFiles.ToList();
         methodFiles.Sort();
         var templatePath = methodFiles.FirstOrDefault();
         return templatePath;
      }

      private IEnumerable<string> GetTemplatesForMethod(string methName)
      {
         var templFiles = GetTemplatesDirectory();
         var methFiles = templFiles.Where(x =>
         {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(x);
            return fileNameWithoutExtension != null && fileNameWithoutExtension.StartsWith(methName);
         });
         return methFiles;
      }

      private static IEnumerable<string> GetTemplatesDirectory()
      {
         var templatePath = AppDomain.CurrentDomain.BaseDirectory + @"Templates\";
         var templFiles = Directory.GetFiles(templatePath);
         return templFiles;
      }

      private void ChangeTemplate()
      {
         if (SelectedMethod == null) return;

         var methodname = SelectedMethod;

         var methInfo = (MethodInfo)(ExternalServiceMethods.FirstOrDefault(x => x.Name == methodname));

         if (methInfo == null) return;
         {
            var pars = methInfo.GetParameters();
            var selTemplatePath = SelectedTemplate;

            var templFiles = GetTemplatesDirectory();
            
            var templatePath = templFiles
               .FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == selTemplatePath);
            if (templatePath != null)
            {
               SetValuesFromTemplate(pars, templatePath);
            }
         }
      }

      private void SetValuesFromTemplate(ParameterInfo[] pars, string templatePath)
      {
         // read template file
         var doc = XDocument.Load(templatePath);
         // find parameter name from xml document nodes
         foreach (var prm in pars)
         {
            var param = doc.Descendants().Where(x => x.Name.LocalName == prm.Name);

            var pcim = ParamItems.First(x => x.Label == prm.Name);
            if (pcim == null) continue;

            var searchParams = param as XElement[] ?? param.ToArray();
            if (pcim.IsSearchParam)
            {
               foreach (var searchParam in searchParams)
               {
                  var temp = searchParam.Elements()
                     .Where(y => y.Name.LocalName == "Parameters")
                     .Elements()
                     .Where(y => y.Name.LocalName == "KeyValueOfstringstring");

                  pcim.SearchParamValue.Clear();

                  foreach (var parm in temp)
                  {
                     var spi = new SearchParameterItem
                     {
                        SearchParamKey = parm
                           .Elements()
                           .First(y => y.Name.LocalName == "Key")
                           .Value,
                        SearchParamValue = parm
                           .Elements()
                           .First(y => y.Name.LocalName == "Value")
                           .Value
                     };



                     pcim.SearchParamValue.Add(spi);
                  }
               }
            }

            if (pcim.IsTextbox)
            {
               pcim.TextboxValue = searchParams.First().Value;
            }

         }
      }
      #endregion

      #region Properties

      public ObservableCollection<ParamCtrlItemModel> ParamItems
      {
         get
         {
            return _paramItems;
         }
         set
         {
            _paramItems = value;
            Set(() => ParamItems, ref  _paramItems, value);
         }
      }
      public Panel PnlPropertyContainer { private get; set; }

      public RichTextBox RichMessage { private get; set; }

      public ObservableCollection<Endpoint> Endpoints { get; private set; }
      public Endpoint SelectedEndpoint
      {
         get { return _selectedEndpoint; }
         set
         {
            Set(() => SelectedEndpoint, ref _selectedEndpoint, value);
         }
      }

      public ObservableCollection<string> MethodNames
      {
         get { return _methodNames; }
         set
         {
            Set(() => MethodNames, ref _methodNames, value);
         }
      }

      public string SelectedMethod
      {
         get { return _selectedMethod; }
         set
         {
            Set(() => SelectedMethod, ref _selectedMethod, value);
            WcfMethodsChanged();
         }
      }

      private List<MemberInfo> ExternalServiceMethods
      {
         get { return _externalServiceMethods; }
         set
         {
            _externalServiceMethods = value;
         }
      }

      public ObservableCollection<string> MethodTemplates
      {
         get { return _methodTemplates; }
         set
         {
            Set(() => MethodTemplates, ref _methodTemplates, value);
         }
      }

      public string SelectedTemplate
      {
         get { return _selectedTemplate; }
         set
         {
            Set(() => SelectedTemplate, ref _selectedTemplate, value);
            ChangeTemplate();
         }
      }



      #endregion

      #region Commands
      private RelayCommand _invokeMethodCommand;

      public RelayCommand InvokeMethodCommand
      {
         get
         {
            return _invokeMethodCommand
                ?? (_invokeMethodCommand = new RelayCommand(
                                       InvokeMethod));
         }
      }

      public void AddNewSearchParam()
      {

         var paramItems = ParamItems;
         foreach (var pcim in paramItems)
         {
            if (!pcim.IsSearchParam) continue;
            var searchParams = pcim.SearchParamValue;

            do
            {
               _newParamCounter++;
            } while (searchParams.First(x => x.SearchParamKey == "testparam" + _newParamCounter) != null);

            searchParams.Add(new SearchParameterItem() { SearchParamKey = "testparam" + _newParamCounter, SearchParamValue = "testparamvalue2" });
         }

      }

      #endregion

      protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
      {
         var handler = PropertyChanged;
         if (handler != null) handler(this, e);
      }
   }
}