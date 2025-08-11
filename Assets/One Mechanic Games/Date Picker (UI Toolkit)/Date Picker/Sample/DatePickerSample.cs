using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace OMG.UI.DatePickerUITK.Sample
{
    public class DatePickerSample : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private TextField _dateInputField;
        private Button _jumpToDateButton;
        private Button _clearSelectedDateButton;
        private Button _clearSelectedDateWithoutNotifyButton;
        private Label _selectedDateLabel;
        private Slider _dropdownListItemHeightSlider;

        private DatePicker _datePicker;

        private void Start() {
            var root = _uiDocument.rootVisualElement;

            _dateInputField = root.Q<TextField>("jump-to-date-input");
            _jumpToDateButton = root.Q<Button>("jump-to-date-button");
            _jumpToDateButton.clicked += JumpToDate;

            _clearSelectedDateButton = root.Q<Button>("clear-selected-button");
            _clearSelectedDateButton.clicked += ClearSelectedDate;

            _clearSelectedDateWithoutNotifyButton = root.Q<Button>("clear-selected-without-notify-button");
            _clearSelectedDateWithoutNotifyButton.clicked += ClearSelectedDateWithoutNotify;

            _selectedDateLabel = root.Q<Label>("selected-date-label");
            _dropdownListItemHeightSlider = root.Q<Slider>("dropdown-list-item-height-slider");
            _dropdownListItemHeightSlider.RegisterValueChangedCallback(ChangeDropdownItemHeight);

            _datePicker = root.Q<DatePicker>();
            _datePicker.RegisterValueChangedCallback(OnDateSelectionChanged);
        }

        private void ChangeDropdownItemHeight(ChangeEvent<float> evt) {
            _datePicker.DropdownListItemHeight = evt.newValue;
        }

        private void OnDateSelectionChanged(ChangeEvent<DateTime?> evt) {
            _selectedDateLabel.text = evt.newValue.HasValue ? evt.newValue.Value.ToString("MMMM dd, yyyy") : "None";
        }

        // The  _selectedDateLabel value does not change in this case.
        private void ClearSelectedDateWithoutNotify() {
            _datePicker.SetValueWithoutNotify(null);
        }

        private void ClearSelectedDate() {
            _datePicker.value = null;
        }

        private void JumpToDate() {
            if (DateTime.TryParseExact(_dateInputField.value, "MMddyyyy", null,
                    System.Globalization.DateTimeStyles.None, out DateTime result)) {
                _datePicker.CurrentDate = result;
            }
            else {
                Console.WriteLine("Invalid date format.");
            }
        }

        private void OnDestroy() {
            _jumpToDateButton.clicked -= JumpToDate;
            _clearSelectedDateButton.clicked -= ClearSelectedDate;
            _clearSelectedDateWithoutNotifyButton.clicked -= ClearSelectedDateWithoutNotify;
            _dropdownListItemHeightSlider.UnregisterValueChangedCallback(ChangeDropdownItemHeight);
            _datePicker.UnregisterValueChangedCallback(OnDateSelectionChanged);
        }
    }
}