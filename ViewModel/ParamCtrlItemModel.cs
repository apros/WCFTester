using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.ObjectModel;

namespace UNWcfTester.ViewModel
{
   public class SearchParameterItem
   {
      public string SearchParamKey { get; set; }
      public string SearchParamValue { get; set; }

   }

   public class ParamCtrlItemModel : ViewModelBase
   {
      public bool IsTextbox { get; set; }
      public bool IsCombobox { get; set; }
      public bool IsDatetime { get; set; }
      public bool IsCheckbox { get; set; }
      public bool IsSearchParam { get; set; }
      public bool IsOtherType { get; set; }
      public string Label { get; set; }

      private string _textboxValue;

      public string TextboxValue
      {
         get { return _textboxValue; }
         set
         {
            Set(() => TextboxValue, ref _textboxValue, value);
         }
      }
      public bool ComboboxValue { get; set; }
      public DateTime DatetimeValue { get; set; }
      public bool CheckboxValue { get; set; }
      public string OtherTypeValue { get; set; }
      public Type OtherTypeType { get; set; }
      int _newParamCounter;

      ObservableCollection<SearchParameterItem> _searchParamValue;
      public ObservableCollection<SearchParameterItem> SearchParamValue
      {
         get { return _searchParamValue; }
         set
         {
            Set(() => SearchParamValue, ref _searchParamValue, value);
         }
      }

      private RelayCommand _addNewSearchParam;
      

      public RelayCommand AddNewSearchParamCommand
      {
         get
         {
            return _addNewSearchParam ?? (_addNewSearchParam = new RelayCommand(
                   AddNewSearchParam
               ));
         }
      }

      private void AddNewSearchParam()
      {
         if (this.IsSearchParam)
         {
            var searchParams = this.SearchParamValue;

            do
            {
               _newParamCounter++;
            } while (searchParams.Contains(new SearchParameterItem() { SearchParamKey = ("testparam" + _newParamCounter) }));

            this.SearchParamValue.Add(new SearchParameterItem() { SearchParamKey = "testparam" + _newParamCounter, SearchParamValue = "testparamvalue2" });

         }
      }
   }
}
