# Management Functions

This component contains the Azure Function (EventGrid trigger) that manages the BlobInventory Completed events from the storage account, parses inventory files (using the inventory manifest to retrieve the list of the JSON files to parse) and inserts statistics into a table storage.