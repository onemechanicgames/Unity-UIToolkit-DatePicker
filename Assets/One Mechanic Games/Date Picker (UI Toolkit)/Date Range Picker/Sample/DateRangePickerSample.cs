using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace OMG.UI.DatePickerUITK.Sample
{
    public class DateRangePickerSample : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        
        private TextField _dateInputField;
        private Button _jumpToDateButton;
        private Button _clearSelectedDateButton;
        private TextField _makeSelectionFromInputField;
        private TextField _makeSelectionToInputField;
        private Button _makeSelectionButton;
        private Label _selectedDateLabel;
        
        private DateRangePicker _dateRangePicker;
        

        private void Start() {
            var root = _uiDocument.rootVisualElement;
            
            _dateInputField = root.Q<TextField>("jump-to-date-input");
            _jumpToDateButton = root.Q<Button>("jump-to-date-button");
            _jumpToDateButton.clicked += JumpToDate;
            
            _clearSelectedDateButton = root.Q<Button>("clear-selected-button");
            _clearSelectedDateButton.clicked += ClearSelectedDate;
            
            _makeSelectionFromInputField = root.Q<TextField>("make-selection-from-input");
            _makeSelectionToInputField = root.Q<TextField>("make-selection-to-input");
            _makeSelectionButton = root.Q<Button>("make-selection-button");
            _makeSelectionButton.clicked += MakeSelection;
            
            _selectedDateLabel = root.Q<Label>("selected-date-label");
            
            _dateRangePicker = root.Q<DateRangePicker>();
            _dateRangePicker.RegisterValueChangedCallback<(DateTime?, DateTime?)>(OnDateRangeSelectionChanged);
        }

        
        private void JumpToDate() {
            
            _dateRangePicker.value = (null, DateTime.Today);
            return;
            if (DateTime.TryParseExact(_dateInputField.value, "MMddyyyy", null, System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                _dateRangePicker.CurrentDate = result;
            }
            else
            {
                Console.WriteLine("Invalid date format.");
            }
        }
        
        private void ClearSelectedDate() {
            _dateRangePicker.value = (null, null);
        }
        
        
        private void MakeSelection() {
            (DateTime?, DateTime?) selection;
            if (DateTime.TryParseExact(_makeSelectionFromInputField.value, "MMddyyyy", null, System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                selection.Item1 = result;
            }
            else
            {
                Console.WriteLine("Invalid date format.");
                return;
            }
            
            if (DateTime.TryParseExact(_makeSelectionToInputField.value, "MMddyyyy", null, System.Globalization.DateTimeStyles.None, out result))
            {
                selection.Item2 = result;
            }
            else
            {
                Console.WriteLine("Invalid date format.");
                return;
            }
            
            _dateRangePicker.value = selection;
        }
        
        
        private void OnDateRangeSelectionChanged(ChangeEvent<(DateTime?, DateTime?)> evt) {
            string fromDateValue = evt.newValue.Item1.HasValue
                ? evt.newValue.Item1.Value.ToString("MMMM dd, yyyy")
                : "None";
            
            string toDateValue = evt.newValue.Item2.HasValue
                ? evt.newValue.Item2.Value.ToString("MMMM dd, yyyy")
                : "None";
            
            _selectedDateLabel.text = $"{fromDateValue} - {toDateValue}";
        }


        private void OnDestroy() {
            _jumpToDateButton.clicked -= JumpToDate;
            _clearSelectedDateButton.clicked -= ClearSelectedDate;
            _makeSelectionButton.clicked -= MakeSelection;
            _dateRangePicker.UnregisterValueChangedCallback<(DateTime?, DateTime?)>(OnDateRangeSelectionChanged);
        }
    }
}