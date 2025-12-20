📅 DatePicker & DateRangePicker for Unity UI Toolkit

A set of custom UI Toolkit components that bring modern, user-friendly date selection to your Unity projects.
Includes:

DatePicker – For selecting a single date.

DateRangePicker – For selecting a start and end date.

Both components are designed for UI Toolkit, fully customizable with USS styles, and easy to integrate into any project.

<br>

✨ Features

Fully integrated with Unity UI Toolkit.

USS styling support for seamless theme integration.

Public API for easy runtime control.

<b>Two example scenes included.</b>

<br>

📦 Installation

Import the package into your Unity project.

Ensure your project is using UI Toolkit (Unity 6.0+ recommended).

The components and styles will be available under:

Assets/One Mechanic Games/Date Picker (UI Toolkit)/PackageResources/Styles

<br>

🎨 Add Styles to Your Runtime Theme

To make the components display correctly, you must reference the USS style files in your Runtime Theme:

Open your UI Toolkit Panel Settings asset.

Locate the Theme assigned under Theme Style Sheets.

Add the provided USS files from:

- Assets/One Mechanic Games/Date Picker (UI Toolkit)/PackageResources/Styles/DatePicker.uss
- Assets/One Mechanic Games/Date Picker (UI Toolkit)/PackageResources/Styles/DateRangePicker.uss

If you have a custom runtime theme, add the above USS files to it as well.
If you are using Unity's default runtime theme, you still need to add these USS references.

⚠ Without adding the USS files, the controls will render unstyled and unusable.

<br>

🛠 Basic Usage

- In UXML

\<DatePicker name="myDatePicker" />

\<DateRangePicker name="myDateRangePicker" />

- In C#

var datePicker = rootVisualElement.Q\<DatePicker>("myDatePicker");
datePicker.RegisterValueChangedCallback(OnDateSelectionChanged);

<br>

📋 Requirements

Unity 6

Tested with Unity 6.

<br>

💡 Tips

You can override the provided USS styles to match your project's theme.

The components are designed to work with runtime UI Toolkit (not Editor UI Toolkit).


📜 License
This asset is licensed for use in your Unity projects as per the Unity Asset Store terms.