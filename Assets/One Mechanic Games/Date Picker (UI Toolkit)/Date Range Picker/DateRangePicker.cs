using System;
using UnityEngine.UIElements;

namespace OMG.UI.DatePickerUITK
{
    [UxmlElement]
    public partial class DateRangePicker : DatePicker, INotifyValueChanged<(DateTime?, DateTime?)>
    {
        /*
         * Root Uss Class
         */
        private const string USSClassName = "omg__date-range-picker";
        
        private const string USSSelectorHighlightClassName = "highlighted";
        
        private DateTime? _startDate;
        private DateTime? _endDate;


        public DateRangePicker() {
            AddToClassList(USSClassName);
        }


       protected override void OnDayLabelClicked(ClickEvent evt) {
            var selectedLabel = (Label)evt.target;
            var selectedDate = GetDateForLabel(selectedLabel);
            
            bool isHandled = HandleStartDateSelection(selectedDate);
            if (!isHandled)
                HandleEndDateSelection(selectedDate);
            
            RenderGrid();
        }

        
        private bool HandleStartDateSelection(DateTime selectedDate) {

            if (!_startDate.HasValue) {
                TryUpdateValueAndNotify(selectedDate, null);
                return true;
            }
            if (_startDate.HasValue && selectedDate == _startDate && ClickAgainToDeselect) {
                TryUpdateValueAndNotify(null, null);
                return true;
            }
            if(_endDate.HasValue){
                TryUpdateValueAndNotify(selectedDate, null);
                return true;
            }

            return false;
        }
        
        
        private void HandleEndDateSelection(DateTime selectedDate) {
            
            if (!_endDate.HasValue) {
                
                if (selectedDate < _startDate) {
                    TryUpdateValueAndNotify(selectedDate, _startDate);
                    return;
                }
                
                TryUpdateValueAndNotify(_startDate, selectedDate);
                return;    
            }
            
            if(_endDate.HasValue && selectedDate == _endDate && ClickAgainToDeselect) {
                TryUpdateValueAndNotify(_startDate, null);
            }
        }
        

        internal override void StyleCellForDate(Label cellLabel, GridDayInfo cellDayInfo) {

            if (cellDayInfo.Date == _startDate || cellDayInfo.Date == _endDate) {
                cellLabel.AddToClassList(USSSelectorSelectedClassName);
            }
            else {
                cellLabel.RemoveFromClassList(USSSelectorSelectedClassName);
            }
            
            if (!_startDate.HasValue || !_endDate.HasValue) {
                cellLabel.RemoveFromClassList(USSSelectorHighlightClassName);
                return;
            }

            if (cellDayInfo.Date > _startDate.Value && cellDayInfo.Date < _endDate.Value )
                cellLabel.AddToClassList(USSSelectorHighlightClassName);
            else
                cellLabel.RemoveFromClassList(USSSelectorHighlightClassName);
        }


#region INotifyValueChanged implementation        
        
        public void SetValueWithoutNotify((DateTime?, DateTime?) newValue) {
            _startDate = newValue.Item1;
            _endDate = newValue.Item2;
            RenderGrid();
        }
        
        
        public new (DateTime?, DateTime?) value { 
            get => (_startDate, _endDate);
            set => TryUpdateValueAndNotify(value.Item1, value.Item2);
        }
        
#endregion
        

        private void TryUpdateValueAndNotify(DateTime? newStartDate, DateTime? newEndDate) {
            if (_startDate.Equals(newStartDate) && _endDate.Equals(newEndDate))
                return;
            
            var newValue = (newStartDate, newEndDate);
            
            using (ChangeEvent<(DateTime?, DateTime?)> evt = ChangeEvent<(DateTime?, DateTime?)>.GetPooled(
                       (_startDate, _endDate), newValue)) {
                evt.target = this;
                SetValueWithoutNotify(newValue);
                SendEvent(evt);
            }
        }
    }
}