# AppTrackerWin

Small Windows application that tracks Excel usage on your computer. Runs in the background using minimal resources, can be minimized to the taskbar.

Once you open and have in focus an Excel file it'll save its name and the time spent with that file open. It saves the name of the files with their time open per day for easier reporting.

Database is the lightweight SQLite and you can clear its content or just remove some entries from it. There is also a possibility to export for a given period.

The application is written in WPF using MVVM design pattern.
