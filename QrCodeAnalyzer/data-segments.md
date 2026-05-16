The main windows should addititionally be able to display the data segments of the process. This is useful for debugging purposes, as it allows the user to see the contents of the data segments and how they are being used by the process.

They should be displayed in a sort of table with two columns:

1. Segment encoding mode
2. Segment content

The table should be towards the bottom of the main window.

The content should be displayed as follows:

- Encoding mode Numeric and Alphanumeric: as text (decoded)
- Encoding mode Byte: If it can be converted to a string using UTF-8, it should be displayed as text (decoded). Otherwise, it should be displayed as a hex dump.
- Encoding mode Kanji: as hex dump
- Encoding mode ECI: display the ECI number
- Other encoding modes: as hex dump

The list of data segments is accessible via the DebugAccess object.

