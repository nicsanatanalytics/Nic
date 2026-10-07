# PDF Content Searcher

A Windows desktop application built with C# and WPF to search for specific text within PDF files across a directory and its subdirectories.

## Features
- Recursive folder scanning.
- Fast text extraction using the `PdfPig` library.
- Displays file names and specific page numbers where matches were found.
- Displays the context (the specific line) where the text was found.
- Export search results to an Excel (.xlsx) file using `ClosedXML`.
- Persistent search history via an editable dropdown menu.
- **New**: Stop/Cancel button to halt long-running search operations.
- Double-click to open the PDF file.
- Modern UI with progress tracking.

## Requirements
- .NET 8.0 SDK or Runtime.
- Windows 10 or 11.

## How to Build and Run
1. Ensure you have the .NET 8 SDK installed.
2. Open a terminal in the project folder.
3. Run `dotnet build` to restore dependencies and compile.
4. Run `dotnet run` to start the application.