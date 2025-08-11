using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OMG.UI.DatePickerUITK
{
    [UxmlElement]
    public partial class DatePicker : VisualElement, INotifyValueChanged<DateTime?>
    {
        /// <NOTE>
        /// UIToolkit's DropdownField creates dropdown lists in Root Panel (or global) context and therefore
        /// would not be part of `DatePicker` hierarchy. As a result, in order to change styles of those
        /// dropdowns lists, we would need to alter global styles, which is not an ideal implementation.
        /// So we create our own dropdown lists, host them under "_popupContainer" container, effectively
        /// recreating the functionality of DropdownField. More on this can be found on this unity
        /// discussion thread : https://discussions.unity.com/t/change-styles-for-dropdownfield/887573/26 
        /// </NOTE>
        
        /*
         * Root Uss Class
         */
        private const string USSClassName = "omg__date-picker";
        /*
         * Header Container Class. The header contains left/right buttons + month dropdown + year dropdown
         */
        private const string USSHeaderClassName = USSClassName + "__header-container";
        /*
         * Left/Right buttons in the header
         */
        private const string USSNavigationButtonClassName = USSHeaderClassName + "__navigation-button";
        /*
         * Dropdown buttons in the header
         */
        private const string USSDropdownClassName = USSHeaderClassName + "__dropdown";
        /*
         * Chevrons/Carets in the dropdown buttons
         */
        private const string USSDropdownChevronClassName = USSDropdownClassName + "__chevron";
        
        /* ============================== HEADER USS CLASSES END ============================== */
        
        /*
         * Dropdown list class
         */
        private const string USSDropdownListClassName = USSClassName + "__dropdown-list";
        
        /* ========================== Dropdown List USS CLASSES END ========================== */
        
        /*
         * Container for week labels "Sun", "Mon" ... "Sat"
         */
        private const string USSWeekLabelsContainerClassName = USSClassName + "__week-labels-container";
        /*
         * Week label class
         */
        private const string USSWeekLabelClassName = USSWeekLabelsContainerClassName + "__week-label";
        
        /* ========================== Week label headings USS CLASSES END ========================== */
        
        /*
         * Container for dates rows
         */
        private const string USSDayGridContainerClassName = USSClassName + "__grid-container";
        /*
         * Container for individual dates in a single row
         */
        private const string USSDayGridRowContainerClassName = USSDayGridContainerClassName + "__row-container";
        /*
         * Class for individual date
         */
        private const string USSDayGridDayClassName = USSDayGridRowContainerClassName + "__grid-day";

        /* ========================== Grid USS CLASSES END ========================== */
        
        private const string USSSelectorMonthClassName = "month";
        private const string USSSelectorYearClassName = "year";
        
        private const string USSSelectorLeftClassName = "left";
        private const string USSSelectorRightClassName = "right";

        protected const string USSSelectorSelectedClassName = "selected";
        private const string USSSelectorThisMonthClassName = "this-month";
        private const string USSSelectorHiddenClassName = "hidden";
        private const string USSSelectorInvisibleClassName = "invisible";
        
        private const string USSSelectorIconClassName = "icon";
        private const string USSSelectorDropdownOpenClassName = "open";
        
        /* ========================== USS SELECTOR CLASSES END ========================== */

        protected const int MaxWeeks = 6;
        protected const int ColumnCount = 7;
        private const int MaxDaysInMonth = 31;

        private static readonly string[] DayLabels = new[] 
            { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

        private static readonly string[] ShortMonthLabels = new[] 
            { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        private static readonly string[] FullMonthLabels = new[]
            { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        
        private Button _previousMonthButton;
        private Button _nextMonthButton;
        private Button _monthDropdown;
        private ListView _monthListView;
        
        private Button _yearDropdown;
        private ListView _yearListView;
        
        /// <summary>
        /// Container for the month and year list views.
        /// </summary>
        private VisualElement _popupContainer;
        
        private static string[] _dayValueLabels;
        private List<string> _yearDropdownValues;
        
        /// <summary>
        /// Min / Max values to show in the year-dropdown. Can be overriden by MinYear and MaxYear properties
        /// </summary>
        private int _minYear = 1990;
        private int _maxYear = 2030;

        /// <summary>
        /// Current format for month labels (short or full)
        /// </summary>
        private MonthLabelFormat _monthLabelFormat;
        
        /// <summary>
        /// Refer to either ShortMonthLabels or FullMonthLabels
        /// </summary>
        private string[] _monthLabels;
        
        /// <summary>
        /// Represents the date showing on the calendar.
        /// </summary>
        private DateTime _currentDate;

        private DateTime? _selectedDate;

        /// <summary>
        /// Data representation of the grid currently showing on screen
        /// </summary>
        internal readonly GridDayInfo[,] DayGrid = new GridDayInfo[MaxWeeks, ColumnCount];
        private readonly Label[,] _dayGridLabels = new Label[MaxWeeks, ColumnCount];
        private readonly Dictionary<Label, GridDayInfo> _labelToDayInfoMapping = new();
        
        private bool _isMonthDropdownOpen;
        private bool _isYearDropdownOpen;
        private bool _clickAgainToDeselect = true;
        private float _dropdownListItemHeight = 30;
        private bool _showCurrentMonthDatesOnly;


        [UxmlAttribute]
        private int MinYear
        {
            get => _minYear;
            set {
                _minYear = value;
                BuildYearsRange();
                _yearDropdown.MarkDirtyRepaint();
            }
        }


        [UxmlAttribute]
        private int MaxYear
        {
            get => _maxYear;
            set {
                _maxYear = value;
                BuildYearsRange();
                _yearDropdown.MarkDirtyRepaint();
            }
        }
        
        
        [UxmlAttribute]
        private MonthLabelFormat MonthLabelFormat
        {
            get => _monthLabelFormat;
            set {
                _monthLabelFormat = value;
                PopulateMonthLabels();
                RenderGrid();
            }
        }

        
        [UxmlAttribute]
        public bool ShowCurrentMonthDatesOnly
        {
            get => _showCurrentMonthDatesOnly;
            set {
                _showCurrentMonthDatesOnly = value;
                PopulateDaysGrid();
                RenderGrid();
                MarkDirtyRepaint();
            }
        }
        

        [UxmlAttribute]
        public bool ClickAgainToDeselect
        {
            get => _clickAgainToDeselect;
            set {
                _clickAgainToDeselect = value;
                RenderGrid();
            }
        }
        
        
        [UxmlAttribute]
        public float DropdownListItemHeight
        {
            get => _dropdownListItemHeight;
            set {
                _dropdownListItemHeight = value;
                SetDropdownListItemsHeight();
            }
        }


        public DateTime CurrentDate
        {
            get => _currentDate;
            set {
                _currentDate = FirstOfMonth(value);
                PopulateDaysGrid();
                RenderGrid();
            }
        }
        

        public DatePicker() {

            AddToClassList(USSClassName);

            InitializeDayInfos();
            PopulateMonthLabels();
            PopulateDayValueLabels();

            BuildLHeader();
            BuildDayLabels();
            BuildGrid();
            BuildPopupContainer();
            
            //Hide the popup for dropdown-lists initially
            Display(_popupContainer, false);

            BuildYearsRange();
            
            //_currentDate must be set before we can populate the grid
            CurrentDate = DateTime.Today;
            PopulateDaysGrid();
            RenderGrid();
            
            RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
        }


        ~DatePicker() {
            _yearDropdown.clicked -= YearDropdownClicked;
            _monthDropdown.clicked -= MonthDropdownClicked;
            _nextMonthButton.clicked -= OnNavigationRight;
            _previousMonthButton.clicked -= OnNavigateLeft;
            _yearListView.selectedIndicesChanged -= OnYearSelected;
            _monthListView.selectedIndicesChanged -= OnMonthSelected;
            _popupContainer.UnregisterCallback<PointerDownEvent>(ClosePopup);
            foreach (var label in _labelToDayInfoMapping.Keys) {
                label.UnregisterCallback<ClickEvent>(OnDayLabelClicked);
            }
        }


        protected virtual void InitializeDayInfos() {
            for (int week = 0; week < MaxWeeks; week++) {
                for (int day = 0; day < ColumnCount; day++) {
                    DayGrid[week, day] = new GridDayInfo();
                }
            }
        }


        private void PopulateMonthLabels() {
            if (_monthLabelFormat == MonthLabelFormat.Short)
                _monthLabels = ShortMonthLabels;
            else
                _monthLabels = FullMonthLabels;
        }
        
        
        /// <summary>
        /// Stores day numbers 1 to 31 in an array to avoid ToString() operations on
        /// day integer values throughout the code.
        /// </summary>
        private static void PopulateDayValueLabels() {
            if (_dayValueLabels != null)
                return;

            _dayValueLabels = new string[MaxDaysInMonth];
            for (int i = 0; i < MaxDaysInMonth; i++) {
                _dayValueLabels[i] = (i + 1).ToString();
            }
        }

        
        /// <summary>
        /// Builds header UIElement hierarchy 
        /// </summary>
        private void BuildLHeader() {
            var headerContainer = new VisualElement { name = USSHeaderClassName };
            headerContainer.AddToClassList(USSHeaderClassName);

            _previousMonthButton = new Button();
            _previousMonthButton.AddToClassList(USSNavigationButtonClassName);
            _previousMonthButton.AddToClassList(USSSelectorLeftClassName);
            _previousMonthButton.clicked += OnNavigateLeft;

            var icon = new VisualElement();
            icon.pickingMode = PickingMode.Ignore;
            icon.AddToClassList(USSSelectorIconClassName);
            _previousMonthButton.Add(icon);

            _monthDropdown = new Button();
            _monthDropdown.clicked += MonthDropdownClicked;
            _monthDropdown.AddToClassList(USSSelectorMonthClassName);
            _monthDropdown.AddToClassList(USSDropdownClassName);
            
            var monthChevron = new VisualElement();
            monthChevron.pickingMode = PickingMode.Ignore;
            monthChevron.AddToClassList(USSDropdownChevronClassName);
            _monthDropdown.Add(monthChevron);

            _yearDropdown = new Button();
            _yearDropdown.clicked += YearDropdownClicked;
            _yearDropdown.AddToClassList(USSSelectorYearClassName);
            _yearDropdown.AddToClassList(USSDropdownClassName);
            
            var yearChevron = new VisualElement();
            yearChevron.pickingMode = PickingMode.Ignore;
            yearChevron.AddToClassList(USSDropdownChevronClassName);
            _yearDropdown.Add(yearChevron);

            _nextMonthButton = new Button();
            _nextMonthButton.AddToClassList(USSNavigationButtonClassName);
            _nextMonthButton.AddToClassList(USSSelectorRightClassName);
            _nextMonthButton.clicked += OnNavigationRight;

            icon = new VisualElement();
            icon.pickingMode = PickingMode.Ignore;
            icon.AddToClassList(USSSelectorIconClassName);
            _nextMonthButton.Add(icon);

            headerContainer.Add(_previousMonthButton);
            headerContainer.Add(_monthDropdown);
            headerContainer.Add(_yearDropdown);
            headerContainer.Add(_nextMonthButton);

            Add(headerContainer);
        }
        
        
        /// <summary>
        /// Builds UIElement hierarchy for week labels (Sun, Mon ... Sat) 
        /// </summary>
        private void BuildDayLabels() {
            var weekDayLabelsContainer = new VisualElement { name = USSWeekLabelsContainerClassName };
            weekDayLabelsContainer.AddToClassList(USSWeekLabelsContainerClassName);

            for (int i = 0; i < DayLabels.Length; i++) {
                var weekDayLabel = new Label(DayLabels[i]);
                weekDayLabel.AddToClassList(USSWeekLabelClassName);
                weekDayLabelsContainer.Add(weekDayLabel);
            }

            Add(weekDayLabelsContainer);
        }


        /// <summary>
        /// Builds UIElement hierarchy for the grid with cells for individual dates 
        /// </summary>
        private void BuildGrid() {
            var gridContainer = new VisualElement { name = USSDayGridContainerClassName };
            gridContainer.AddToClassList(USSDayGridContainerClassName);

            for (int week = 0; week < MaxWeeks; week++) {

                var gridDaysRow = new VisualElement();
                gridDaysRow.AddToClassList(USSDayGridRowContainerClassName);

                for (int day = 0; day < ColumnCount; day++) {
                    var dayLabel = new Label();
                    dayLabel.usageHints = UsageHints.DynamicColor;
                    dayLabel.RegisterCallback<ClickEvent>(OnDayLabelClicked);
                    dayLabel.AddToClassList(USSDayGridDayClassName);
                    gridDaysRow.Add(dayLabel);
                    _dayGridLabels[week, day] = dayLabel;
                }

                gridContainer.Add(gridDaysRow);
            }
            
            Add(gridContainer);
        }
        
        
        /// <summary>
        /// Builds the container for month and year list views
        /// </summary>
        private void BuildPopupContainer() {
            _popupContainer = new VisualElement();
            _popupContainer.style.position = Position.Absolute;
            _popupContainer.style.left = 0;
            _popupContainer.style.top = 0;
            _popupContainer.style.right = 0;
            _popupContainer.style.bottom = 0;
            _popupContainer.RegisterCallback<PointerDownEvent>(ClosePopup);

            _monthListView = new ListView();
            _monthListView.style.position = Position.Absolute;
            _monthListView.AddToClassList(USSSelectorMonthClassName);
            _monthListView.AddToClassList(USSDropdownListClassName);
            _monthListView.itemsSource = _monthLabels;
            _monthListView.selectionType = SelectionType.Single;
            _monthListView.makeItem = () => new Label();
            _monthListView.bindItem = (visualElement, index) => {
                var label = (Label)visualElement;
                label.text = _monthLabels[index];
            };
            _monthListView.selectedIndicesChanged += OnMonthSelected;
            
            _yearListView = new ListView();
            _yearListView.style.position = Position.Absolute;
            _yearListView.AddToClassList(USSSelectorYearClassName);
            _yearListView.AddToClassList(USSDropdownListClassName);
            _yearListView.selectionType = SelectionType.Single;
            _yearListView.makeItem = () => new Label();
            _yearListView.bindItem = (visualElement, index) => {
                var label = (Label)visualElement;
                label.text = _yearDropdownValues[index];
            };
            _yearListView.selectedIndicesChanged += OnYearSelected;

            SetDropdownListItemsHeight();
                
            _popupContainer.Add(_monthListView);
            _popupContainer.Add(_yearListView);
            Add(_popupContainer);
        }


        private void SetDropdownListItemsHeight() {
            _monthListView.fixedItemHeight = _dropdownListItemHeight;
            _yearListView.fixedItemHeight = _dropdownListItemHeight;
            _monthListView.Rebuild();
            _yearListView.Rebuild();
        }
        
        
        private void Display(VisualElement visualElement, bool display) {
            visualElement.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        
        /// <summary>
        /// Builds the item source for year-dropdown list. 
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        private void BuildYearsRange() {
            int count = _maxYear - _minYear + 1;

            if (count <= 0) {
                var exception = new ArgumentException("Invalid year range.");
                Debug.LogException(exception);
                throw exception;
            }

            _yearDropdownValues = new List<string>(count);

            for (int i = 0; i < count; i++)
                _yearDropdownValues.Add($"{_maxYear - i}");

            _yearListView.itemsSource = _yearDropdownValues;
            _yearListView.Rebuild();
        }
        
        
        /// <summary>
        /// Based on the current month (_currentDate), builds a grid of `GridDayInfo` objects
        /// </summary>
        private void PopulateDaysGrid() {

            DateTime firstDayOfMonth = _currentDate;
            int startDayOfWeek = -(int)firstDayOfMonth.DayOfWeek;

            for (int week = 0; week < MaxWeeks; week++) {

                bool isHidden = week > 0 && _currentDate.AddDays(startDayOfWeek).Month != _currentDate.Month;

                for (int day = 0; day < ColumnCount; day++) {

                    var thisDate = _currentDate.AddDays(startDayOfWeek);
                    var isInThisMonth = thisDate.Month == _currentDate.Month;

                    SetDayInfo(week, day, thisDate, isInThisMonth, isHidden);

                    startDayOfWeek++;
                }
            }
        }

        
        private void SetDayInfo(int week, int day, DateTime date, bool belongsToCurrentMonth, bool shouldHide) {
            GridDayInfo dayInfo = DayGrid[week, day];
            dayInfo.Date = date;
            dayInfo.BelongsToCurrentMonth = belongsToCurrentMonth;
            dayInfo.ShouldHide = shouldHide;
            dayInfo.ShouldInvisible = ShowCurrentMonthDatesOnly && !shouldHide;
        }
        


        /// <summary>
        /// Presents `GridDayInfo` data onto the UI
        /// </summary>
        protected void RenderGrid() {

            _monthDropdown.text = _monthLabels[_currentDate.Month - 1];
            _yearDropdown.text = _currentDate.Year.ToString();

            for (int week = 0; week < MaxWeeks; week++) {
                for (int day = 0; day < ColumnCount; day++) {

                    var cellDayInfo = DayGrid[week, day];
                    var cellDay = cellDayInfo.Date.Day;
                    var cellLabelValue = _dayValueLabels[cellDay - 1];
                    var cellLabel = _dayGridLabels[week, day];

                    _labelToDayInfoMapping[cellLabel] = cellDayInfo;
                    cellLabel.text = cellLabelValue;

                    if (cellDayInfo.ShouldHide) {
                        cellLabel.AddToClassList(USSSelectorHiddenClassName);
                    }
                    else {
                        cellLabel.RemoveFromClassList(USSSelectorHiddenClassName);
                    }
                    
                    if (cellDayInfo.ShouldInvisible) {
                        cellLabel.AddToClassList(USSSelectorInvisibleClassName);
                    }
                    else {
                        cellLabel.RemoveFromClassList(USSSelectorInvisibleClassName);
                    }

                    if (cellDayInfo.BelongsToCurrentMonth) {
                        cellLabel.AddToClassList(USSSelectorThisMonthClassName);
                    }
                    else {
                        cellLabel.RemoveFromClassList(USSSelectorThisMonthClassName);
                    }

                    StyleCellForDate(cellLabel, cellDayInfo);
                }
            }
        }

        internal virtual void StyleCellForDate(Label cellLabel, GridDayInfo cellDayInfo) {
            if (_selectedDate.HasValue && _selectedDate.Value == cellDayInfo.Date) {
                cellLabel.AddToClassList(USSSelectorSelectedClassName);
            }
            else {
                cellLabel.RemoveFromClassList(USSSelectorSelectedClassName);
            }
        }
        
        
        /// <summary>
        /// Places months and year dropdown lists under their respective buttons in the
        /// DatePicker header
        /// </summary>
        private void OnGeometryChangedEvent(GeometryChangedEvent geometryChangedEvent) {
            var rootPos = this.LocalToWorld(Vector2.zero);
            
            var dropdownPosition = _monthDropdown.LocalToWorld(Vector2.zero);
            var popupStyle = _monthListView.style;
            popupStyle.top = dropdownPosition.y - rootPos.y + _monthDropdown.layout.height;
            popupStyle.left = dropdownPosition.x - rootPos.x;
            popupStyle.width = _monthDropdown.worldBound.width + 10;
            
            dropdownPosition = _yearDropdown.LocalToWorld(Vector2.zero);
            popupStyle = _yearListView.style;
            popupStyle.top = dropdownPosition.y - rootPos.y + _yearDropdown.layout.height;
            popupStyle.left = dropdownPosition.x - rootPos.x;
            popupStyle.width = _yearDropdown.worldBound.width + 10;
        }

#region User interaction callback methods

        private void OnNavigateLeft() {
            CurrentDate = _currentDate.AddMonths(-1);
        }

        
        private void OnNavigationRight() {
            CurrentDate = _currentDate.AddMonths(1);
        }
        
        
        private void OnMonthSelected(IEnumerable<int> enumerable) {
            int selectedIndex = enumerable.First();
            CurrentDate = new DateTime(_currentDate.Year, selectedIndex + 1, _currentDate.Day);
        }
        
        
        private void OnYearSelected(IEnumerable<int> enumerable) {
            int selectedIndex = enumerable.First();
            CurrentDate = new DateTime(Convert.ToInt32(_yearDropdownValues[selectedIndex]), _currentDate.Month, _currentDate.Day);
        }


        protected virtual void OnDayLabelClicked(ClickEvent evt) {
            var selectedLabel = (Label)evt.target;
            var selectedDate = GetDateForLabel(selectedLabel);

            //If the same date is clicked, unset it if ClickAgainToDeselect is set to true
            if (ClickAgainToDeselect && _selectedDate.HasValue && _selectedDate.Value == selectedDate)
                TryUpdateValueAndNotify(null);
            else
                TryUpdateValueAndNotify(selectedDate);

            RenderGrid();
        }

        
        private void MonthDropdownClicked() {
            Display(_yearListView, false);
            Display(_monthListView, !_isMonthDropdownOpen);
            Display(_popupContainer, !_isMonthDropdownOpen);
            _isMonthDropdownOpen = !_isMonthDropdownOpen;

            ReflectChevronState();
        }

        
        private void YearDropdownClicked() {
            Display(_monthListView, false);
            Display(_yearListView, !_isYearDropdownOpen);
            Display(_popupContainer, !_isYearDropdownOpen);
            _isYearDropdownOpen = !_isYearDropdownOpen;

            ReflectChevronState();
        }

        
        private void ClosePopup(PointerDownEvent evt) {
            Display(_monthListView, false);
            Display(_yearListView, false);
            Display(_popupContainer, false);
            _isMonthDropdownOpen = false;
            _isYearDropdownOpen = false;

            ReflectChevronState();
        }
        
#endregion


        private void ReflectChevronState() {
                    
            if (_isMonthDropdownOpen)
                _monthDropdown.AddToClassList(USSSelectorDropdownOpenClassName);
            else
                _monthDropdown.RemoveFromClassList(USSSelectorDropdownOpenClassName);
                    
            if (_isYearDropdownOpen)
                _yearDropdown.AddToClassList(USSSelectorDropdownOpenClassName);
            else
                _yearDropdown.RemoveFromClassList(USSSelectorDropdownOpenClassName);
        }
        

#region INotifyValueChanged implementation

        public void SetValueWithoutNotify(DateTime? newValue) {
            _selectedDate = newValue;
            RenderGrid();
        }


        public DateTime? value
        {
            get => _selectedDate;
            set => TryUpdateValueAndNotify(value);
        }

#endregion

        private void TryUpdateValueAndNotify(DateTime? newValue) {
            if (_selectedDate.Equals(newValue))
                return;
            using (ChangeEvent<DateTime?> evt = ChangeEvent<DateTime?>.GetPooled(_selectedDate, newValue)) {
                evt.target = this;
                SetValueWithoutNotify(newValue);
                SendEvent(evt);
            }
        }


        protected DateTime GetDateForLabel(Label label) {
            return _labelToDayInfoMapping[label].Date;
        }

        private DateTime FirstOfMonth(DateTime dateTime) => new DateTime(dateTime.Year, dateTime.Month, 1);
    }
}
