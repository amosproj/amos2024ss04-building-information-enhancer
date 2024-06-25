// Switch to the target database
db = db.getSiblingDB("bci-metadata");

// Create read only user
db.createUser({
  user: "readonly",
  pwd: "readonly",
  roles: [{ role: "read", db: "bci-metadata" }],
});

db.createUser({
  user: "datapipeline",
  pwd: "datapipeline",
  roles: [{ role: "readWrite", db: "bci-metadata" }],
});

// Define the datasets
const datasets = [
  {
    metadataBasicData: {
      DatasetId: "empty_map",
      Name: "Empty Map",
      Description: "An empty, default map of Germany, with no data loaded.",
    },
    metadataAdditionalData: {
      Type: "marker",
      MinZoomLevel: -1,
      Tables: [],
    },
  },
  {
    metadataBasicData: {
      DatasetId: "EV_charging_stations",
      Name: "Charging stations",
      Description: "Locations of all charging stations in Germany.",
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" fill="#000000" viewBox="0 0 256 256"><path d="M134.62,123.51a8,8,0,0,1,.81,7.46l-16,40A8,8,0,0,1,104.57,165l11.61-29H96a8,8,0,0,1-7.43-11l16-40A8,8,0,1,1,119.43,91l-11.61,29H128A8,8,0,0,1,134.62,123.51ZM248,86.63V168a24,24,0,0,1-48,0V128a8,8,0,0,0-8-8H176v88h16a8,8,0,0,1,0,16H32a8,8,0,0,1,0-16H48V56A24,24,0,0,1,72,32h80a24,24,0,0,1,24,24v48h16a24,24,0,0,1,24,24v40a8,8,0,0,0,16,0V86.63A8,8,0,0,0,229.66,81L210.34,61.66a8,8,0,0,1,11.32-11.32L241,69.66A23.85,23.85,0,0,1,248,86.63ZM160,208V56a8,8,0,0,0-8-8H72a8,8,0,0,0-8,8V208Z"></path></svg>',
    },
    metadataAdditionalData: {
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" fill="#000000" viewBox="0 0 256 256"><path d="M134.62,123.51a8,8,0,0,1,.81,7.46l-16,40A8,8,0,0,1,104.57,165l11.61-29H96a8,8,0,0,1-7.43-11l16-40A8,8,0,1,1,119.43,91l-11.61,29H128A8,8,0,0,1,134.62,123.51ZM248,86.63V168a24,24,0,0,1-48,0V128a8,8,0,0,0-8-8H176v88h16a8,8,0,0,1,0,16H32a8,8,0,0,1,0-16H48V56A24,24,0,0,1,72,32h80a24,24,0,0,1,24,24v48h16a24,24,0,0,1,24,24v40a8,8,0,0,0,16,0V86.63A8,8,0,0,0,229.66,81L210.34,61.66a8,8,0,0,1,11.32-11.32L241,69.66A23.85,23.85,0,0,1,248,86.63ZM160,208V56a8,8,0,0,0-8-8H72a8,8,0,0,0-8,8V208Z"></path></svg>',
      Type: "marker",
      MinZoomLevel: 10,
      DisplayProperty: "name",
      Tables: [],
    },
  },
  {
    metadataBasicData: {
      DatasetId: "house_footprints",
      Name: "House Footprints",
      Description: "Footprints for the houses.",
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" fill="#000000" viewBox="0 0 256 256"><path d="M232,56H72V40a8,8,0,0,0-8-8H48A32,32,0,0,0,16,64V176a32,32,0,0,0,32,32H232a8,8,0,0,0,8-8V64A8,8,0,0,0,232,56ZM32,64A16,16,0,0,1,48,48h8v96H48a31.82,31.82,0,0,0-16,4.29ZM224,192H48a16,16,0,0,1,0-32H64a8,8,0,0,0,8-8V72H224ZM104,136a8,8,0,0,0,0,16h16v8a8,8,0,0,0,16,0v-8h24v8a8,8,0,0,0,16,0v-8h16a8,8,0,0,0,0-16H176V120h16a8,8,0,0,0,0-16H176V96a8,8,0,0,0-16,0v8H136V96a8,8,0,0,0-16,0v8H104a8,8,0,0,0,0,16h16v16Zm32-16h24v16H136Z"></path></svg>',
    },
    metadataAdditionalData: {
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" fill="#000000" viewBox="0 0 256 256"><path d="M232,56H72V40a8,8,0,0,0-8-8H48A32,32,0,0,0,16,64V176a32,32,0,0,0,32,32H232a8,8,0,0,0,8-8V64A8,8,0,0,0,232,56ZM32,64A16,16,0,0,1,48,48h8v96H48a31.82,31.82,0,0,0-16,4.29ZM224,192H48a16,16,0,0,1,0-32H64a8,8,0,0,0,8-8V72H224ZM104,136a8,8,0,0,0,0,16h16v8a8,8,0,0,0,16,0v-8h24v8a8,8,0,0,0,16,0v-8h16a8,8,0,0,0,0-16H176V120h16a8,8,0,0,0,0-16H176V96a8,8,0,0,0-16,0v8H136V96a8,8,0,0,0-16,0v8H104a8,8,0,0,0,0,16h16v16Zm32-16h24v16H136Z"></path></svg>',
      Type: "marker",
      MinZoomLevel: 10,
      Tables: [],
    },
  },
  {
    metadataBasicData: {
      DatasetId: "actual_use",
      Name: "Actual Use",
      Description: "The Actual Use (?)",
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" fill="#000000" viewBox="0 0 256 256"><path d="M232,56H72V40a8,8,0,0,0-8-8H48A32,32,0,0,0,16,64V176a32,32,0,0,0,32,32H232a8,8,0,0,0,8-8V64A8,8,0,0,0,232,56ZM32,64A16,16,0,0,1,48,48h8v96H48a31.82,31.82,0,0,0-16,4.29ZM224,192H48a16,16,0,0,1,0-32H64a8,8,0,0,0,8-8V72H224ZM104,136a8,8,0,0,0,0,16h16v8a8,8,0,0,0,16,0v-8h24v8a8,8,0,0,0,16,0v-8h16a8,8,0,0,0,0-16H176V120h16a8,8,0,0,0,0-16H176V96a8,8,0,0,0-16,0v8H136V96a8,8,0,0,0-16,0v8H104a8,8,0,0,0,0,16h16v16Zm32-16h24v16H136Z"></path></svg>',
    },
    metadataAdditionalData: {
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" fill="#000000" viewBox="0 0 256 256"><path d="M232,56H72V40a8,8,0,0,0-8-8H48A32,32,0,0,0,16,64V176a32,32,0,0,0,32,32H232a8,8,0,0,0,8-8V64A8,8,0,0,0,232,56ZM32,64A16,16,0,0,1,48,48h8v96H48a31.82,31.82,0,0,0-16,4.29ZM224,192H48a16,16,0,0,1,0-32H64a8,8,0,0,0,8-8V72H224ZM104,136a8,8,0,0,0,0,16h16v8a8,8,0,0,0,16,0v-8h24v8a8,8,0,0,0,16,0v-8h16a8,8,0,0,0,0-16H176V120h16a8,8,0,0,0,0-16H176V96a8,8,0,0,0-16,0v8H136V96a8,8,0,0,0-16,0v8H104a8,8,0,0,0,0,16h16v16Zm32-16h24v16H136Z"></path></svg>',
      Type: "marker",
      MinZoomLevel: 10,
      Tables: [],
    },
  },
];

// Iterate over datasets and insert only if DatasetId does not exist
datasets.forEach((dataset) => {
  if (
    !db.datasets.findOne({
      "metadataBasicData.DatasetId": dataset.metadataBasicData.DatasetId,
    })
  ) {
    db.datasets.insertOne(dataset);
  }
});
