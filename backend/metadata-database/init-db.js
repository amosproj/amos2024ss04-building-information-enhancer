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
    basicData: {
      DatasetId: "empty_map",
      Name: "Empty Map",
      ShortDescription: `An empty, default map of Germany, with no data loaded.`,
    },
    additionalData: {
      Type: "none",
      DataType: "none",
      LongDescription: `An empty, default map of Germany, with no data loaded. Useful for exploring the map.`,
      MinZoomLevel: -1,
      MarkersThreshold: -1,
      DisplayProperty: [],
      PolygonColoring: null,
      Tables: [],
    },
  },
  {
    basicData: {
      DatasetId: "EV_charging_stations",
      Name: "Charging stations",
      ShortDescription: `Locations of all charging stations in Germany.`,
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" fill="#000000" viewBox="0 0 256 256"><path d="M134.62,123.51a8,8,0,0,1,.81,7.46l-16,40A8,8,0,0,1,104.57,165l11.61-29H96a8,8,0,0,1-7.43-11l16-40A8,8,0,1,1,119.43,91l-11.61,29H128A8,8,0,0,1,134.62,123.51ZM248,86.63V168a24,24,0,0,1-48,0V128a8,8,0,0,0-8-8H176v88h16a8,8,0,0,1,0,16H32a8,8,0,0,1,0-16H48V56A24,24,0,0,1,72,32h80a24,24,0,0,1,24,24v48h16a24,24,0,0,1,24,24v40a8,8,0,0,0,16,0V86.63A8,8,0,0,0,229.66,81L210.34,61.66a8,8,0,0,1,11.32-11.32L241,69.66A23.85,23.85,0,0,1,248,86.63ZM160,208V56a8,8,0,0,0-8-8H72a8,8,0,0,0-8,8V208Z"></path></svg>',
    },
    additionalData: {
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" fill="#000000" viewBox="0 0 256 256"><path d="M134.62,123.51a8,8,0,0,1,.81,7.46l-16,40A8,8,0,0,1,104.57,165l11.61-29H96a8,8,0,0,1-7.43-11l16-40A8,8,0,1,1,119.43,91l-11.61,29H128A8,8,0,0,1,134.62,123.51ZM248,86.63V168a24,24,0,0,1-48,0V128a8,8,0,0,0-8-8H176v88h16a8,8,0,0,1,0,16H32a8,8,0,0,1,0-16H48V56A24,24,0,0,1,72,32h80a24,24,0,0,1,24,24v48h16a24,24,0,0,1,24,24v40a8,8,0,0,0,16,0V86.63A8,8,0,0,0,229.66,81L210.34,61.66a8,8,0,0,1,11.32-11.32L241,69.66A23.85,23.85,0,0,1,248,86.63ZM160,208V56a8,8,0,0,0-8-8H72a8,8,0,0,0-8,8V208Z"></path></svg>',
      Type: "markers",
      DataType: "CSV",
      LongDescription: `A map of EV charging stations displays the locations of electric vehicle charging points located in Germany, helping drivers plan routes and manage charging needs. It is essential for supporting the adoption and convenience of electric vehicles.`,
      MinZoomLevel: 11,
      MarkersThreshold: -1,
      DisplayProperty: [{ displayName: "Operator", value: "operator" }],
      PolygonColoring: null,
      Tables: [],
    },
  },
  {
    basicData: {
      DatasetId: "house_footprints",
      Name: "House Footprints",
      ShortDescription: `Footprints for the houses.`,
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" fill="#000000" viewBox="0 0 256 256"><path d="M232,56H72V40a8,8,0,0,0-8-8H48A32,32,0,0,0,16,64V176a32,32,0,0,0,32,32H232a8,8,0,0,0,8-8V64A8,8,0,0,0,232,56ZM32,64A16,16,0,0,1,48,48h8v96H48a31.82,31.82,0,0,0-16,4.29ZM224,192H48a16,16,0,0,1,0-32H64a8,8,0,0,0,8-8V72H224ZM104,136a8,8,0,0,0,0,16h16v8a8,8,0,0,0,16,0v-8h24v8a8,8,0,0,0,16,0v-8h16a8,8,0,0,0,0-16H176V120h16a8,8,0,0,0,0-16H176V96a8,8,0,0,0-16,0v8H136V96a8,8,0,0,0-16,0v8H104a8,8,0,0,0,0,16h16v16Zm32-16h24v16H136Z"></path></svg>',
    },
    additionalData: {
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" fill="#000000" viewBox="0 0 256 256"><path d="M232,56H72V40a8,8,0,0,0-8-8H48A32,32,0,0,0,16,64V176a32,32,0,0,0,32,32H232a8,8,0,0,0,8-8V64A8,8,0,0,0,232,56ZM32,64A16,16,0,0,1,48,48h8v96H48a31.82,31.82,0,0,0-16,4.29ZM224,192H48a16,16,0,0,1,0-32H64a8,8,0,0,0,8-8V72H224ZM104,136a8,8,0,0,0,0,16h16v8a8,8,0,0,0,16,0v-8h24v8a8,8,0,0,0,16,0v-8h16a8,8,0,0,0,0-16H176V120h16a8,8,0,0,0,0-16H176V96a8,8,0,0,0-16,0v8H136V96a8,8,0,0,0-16,0v8H104a8,8,0,0,0,0,16h16v16Zm32-16h24v16H136Z"></path></svg>',
      Type: "areas",
      DataType: "SHAPE",
      LongDescription: `House footprints refer to the outline or ground area covered by a house, typically measured from the exterior walls of the structure. This footprint includes all parts of the house that are in contact with the ground, and is important for planning and zoning purposes, calculating property taxes, and designing land use.`,
      MinZoomLevel: 11,
      MarkersThreshold: 17,
      DisplayProperty: [],
      PolygonColoring: null,
      Tables: [],
    },
  },
  {
    basicData: {
      DatasetId: "actual_use",
      Name: "Actual Use",
      ShortDescription: `The division of the land based on its utilization.`,
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="#000000" viewBox="0 0 256 256"><path d="M136.83,220.43a8,8,0,0,1-11.09,2.23A183.15,183.15,0,0,0,24,192a8,8,0,0,1,0-16,199.11,199.11,0,0,1,110.6,33.34A8,8,0,0,1,136.83,220.43ZM24,144a8,8,0,0,0,0,16,214.81,214.81,0,0,1,151.17,61.71,8,8,0,1,0,11.2-11.42A230.69,230.69,0,0,0,24,144Zm208,16a216.51,216.51,0,0,0-48.59,5.49q8.24,6.25,16,13.16A201.53,201.53,0,0,1,232,176a8,8,0,0,1,0,16c-6,0-11.93.29-17.85.86q8.32,8.67,15.94,18.14a8,8,0,1,1-12.48,10A247,247,0,0,0,24,128a8,8,0,0,1,0-16,266.33,266.33,0,0,1,48,4.37V80a8,8,0,0,1,3.2-6.4l64-48a8,8,0,0,1,9.6,0l64,48A8,8,0,0,1,216,80v32.49c5.31-.31,10.64-.49,16-.49a8,8,0,0,1,0,16,246.3,246.3,0,0,0-84.26,14.69q9.44,5,18.46,10.78A232.2,232.2,0,0,1,232,144a8,8,0,0,1,0,16ZM120,88h48a8,8,0,0,1,8,8v21.94q11.88-2.56,24-4V84L144,42,88,84v35.81q12.19,3,24,7.18V96A8,8,0,0,1,120,88Zm8.07,45.27A262.48,262.48,0,0,1,160,121.94V104H128v29.24Z"></path></svg>',
    },
    additionalData: {
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="#000000" viewBox="0 0 256 256"><path d="M136.83,220.43a8,8,0,0,1-11.09,2.23A183.15,183.15,0,0,0,24,192a8,8,0,0,1,0-16,199.11,199.11,0,0,1,110.6,33.34A8,8,0,0,1,136.83,220.43ZM24,144a8,8,0,0,0,0,16,214.81,214.81,0,0,1,151.17,61.71,8,8,0,1,0,11.2-11.42A230.69,230.69,0,0,0,24,144Zm208,16a216.51,216.51,0,0,0-48.59,5.49q8.24,6.25,16,13.16A201.53,201.53,0,0,1,232,176a8,8,0,0,1,0,16c-6,0-11.93.29-17.85.86q8.32,8.67,15.94,18.14a8,8,0,1,1-12.48,10A247,247,0,0,0,24,128a8,8,0,0,1,0-16,266.33,266.33,0,0,1,48,4.37V80a8,8,0,0,1,3.2-6.4l64-48a8,8,0,0,1,9.6,0l64,48A8,8,0,0,1,216,80v32.49c5.31-.31,10.64-.49,16-.49a8,8,0,0,1,0,16,246.3,246.3,0,0,0-84.26,14.69q9.44,5,18.46,10.78A232.2,232.2,0,0,1,232,144a8,8,0,0,1,0,16ZM120,88h48a8,8,0,0,1,8,8v21.94q11.88-2.56,24-4V84L144,42,88,84v35.81q12.19,3,24,7.18V96A8,8,0,0,1,120,88Zm8.07,45.27A262.48,262.48,0,0,1,160,121.94V104H128v29.24Z"></path></svg>',
      Type: "areas",
      DataType: "SHAPE",
      LongDescription: `The Actual Use map describes the use of the earth's surface in four main groups (settlement, traffic, vegetation and water bodies). The division of these main groups into almost 140 different types of use, such as residential areas, road traffic, agriculture or flowing water, enables detailed evaluations and analyses of the use of the earth's surface.`,
      MinZoomLevel: 11,
      MarkersThreshold: 15,
      DisplayProperty: [],
      PolygonColoring: {
        attributeName: "nutzart",
        colors: [
          {
            color: "DarkOrchid",
            values: [
              "Wohnbaufläche",
              "Industrie- und Gewerbefläche",
              "Halde",
              "Bergbaubetrieb",
              "Tagebau, Grube Steinbruch",
              "Fläche gemischter Nutzung",
              "Fläche besonderer funktionaler Prägung",
              "Sport-, Freizeit- und Erholungsfläche",
              "Friedhof",
            ],
          },
          {
            color: "green",
            values: ["Wald", "Gehölz", "Sumpf"],
          },
          {
            color: "yellow",
            values: [
              "Landwirtschaft",
              "Heide",
              "Moor",
              "Unland/Vegetationslose Fläche",
            ],
          },
          {
            color: "RosyBrown",
            values: [
              "Straßenverkehr",
              "Weg",
              "Platz",
              "Bahnverkehr",
              "Flugverkehr",
              "Schiffsverkehr",
            ],
          },
          {
            color: "Cyan",
            values: ["Fließgewässer", "Hafenbecken", "Stehendes Gewässer"],
          },
        ],
      },
      Tables: [],
    },
  },
  {
    basicData: {
      DatasetId: "building_models",
      Name: "Building Models",
      ShortDescription: `Simplified 3D building models.`,
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="#000000" viewBox="0 0 256 256"><path d="M232,224H208V32h8a8,8,0,0,0,0-16H40a8,8,0,0,0,0,16h8V224H24a8,8,0,0,0,0,16H232a8,8,0,0,0,0-16ZM64,32H192V224H160V184a8,8,0,0,0-8-8H104a8,8,0,0,0-8,8v40H64Zm80,192H112V192h32ZM88,64a8,8,0,0,1,8-8h16a8,8,0,0,1,0,16H96A8,8,0,0,1,88,64Zm48,0a8,8,0,0,1,8-8h16a8,8,0,0,1,0,16H144A8,8,0,0,1,136,64ZM88,104a8,8,0,0,1,8-8h16a8,8,0,0,1,0,16H96A8,8,0,0,1,88,104Zm48,0a8,8,0,0,1,8-8h16a8,8,0,0,1,0,16H144A8,8,0,0,1,136,104ZM88,144a8,8,0,0,1,8-8h16a8,8,0,0,1,0,16H96A8,8,0,0,1,88,144Zm48,0a8,8,0,0,1,8-8h16a8,8,0,0,1,0,16H144A8,8,0,0,1,136,144Z"></path></svg>',
    },
    additionalData: {
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="#000000" viewBox="0 0 256 256"><path d="M232,224H208V32h8a8,8,0,0,0,0-16H40a8,8,0,0,0,0,16h8V224H24a8,8,0,0,0,0,16H232a8,8,0,0,0,0-16ZM64,32H192V224H160V184a8,8,0,0,0-8-8H104a8,8,0,0,0-8,8v40H64Zm80,192H112V192h32ZM88,64a8,8,0,0,1,8-8h16a8,8,0,0,1,0,16H96A8,8,0,0,1,88,64Zm48,0a8,8,0,0,1,8-8h16a8,8,0,0,1,0,16H144A8,8,0,0,1,136,64ZM88,104a8,8,0,0,1,8-8h16a8,8,0,0,1,0,16H96A8,8,0,0,1,88,104Zm48,0a8,8,0,0,1,8-8h16a8,8,0,0,1,0,16H144A8,8,0,0,1,136,104ZM88,144a8,8,0,0,1,8-8h16a8,8,0,0,1,0,16H96A8,8,0,0,1,88,144Zm48,0a8,8,0,0,1,8-8h16a8,8,0,0,1,0,16H144A8,8,0,0,1,136,144Z"></path></svg>',
      Type: "areas",
      DataType: "CITYGML",
      LongDescription: `The building models have a 3D object of each building plus additional information on its dimentions.`,
      MinZoomLevel: 11,
      MarkersThreshold: 17,
      DisplayProperty: [],
      PolygonColoring: null,
      Tables: [],
    },
  },
  {
    basicData: {
      DatasetId: "air_pollutants",
      Name: "Air Pollutants",
      ShortDescription: `Air measurements from stations across Germany.`,
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="#000000" viewBox="0 0 256 256"><path d="M184,184a32,32,0,0,1-32,32c-13.7,0-26.95-8.93-31.5-21.22a8,8,0,0,1,15-5.56C137.74,195.27,145,200,152,200a16,16,0,0,0,0-32H40a8,8,0,0,1,0-16H152A32,32,0,0,1,184,184Zm-64-80a32,32,0,0,0,0-64c-13.7,0-26.95,8.93-31.5,21.22a8,8,0,0,0,15,5.56C105.74,60.73,113,56,120,56a16,16,0,0,1,0,32H24a8,8,0,0,0,0,16Zm88-32c-13.7,0-26.95,8.93-31.5,21.22a8,8,0,0,0,15,5.56C193.74,92.73,201,88,208,88a16,16,0,0,1,0,32H32a8,8,0,0,0,0,16H208a32,32,0,0,0,0-64Z"></path></svg>',
    },
    additionalData: {
      Type: "markers",
      DataType: "CSV",
      Icon: '<svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="#000000" viewBox="0 0 256 256"><path d="M240,208H224V96a16,16,0,0,0-16-16H144V32a16,16,0,0,0-24.88-13.32L39.12,72A16,16,0,0,0,32,85.34V208H16a8,8,0,0,0,0,16H240a8,8,0,0,0,0-16ZM208,96V208H144V96ZM48,85.34,128,32V208H48ZM112,112v16a8,8,0,0,1-16,0V112a8,8,0,1,1,16,0Zm-32,0v16a8,8,0,0,1-16,0V112a8,8,0,1,1,16,0Zm0,56v16a8,8,0,0,1-16,0V168a8,8,0,0,1,16,0Zm32,0v16a8,8,0,0,1-16,0V168a8,8,0,0,1,16,0Z"></path></svg>',
      LongDescription: ` The air quality dataset provides air measurements from stations across Germany. This data includes various air pollutants, such as particulate matter (PM10 and PM2'5) and nitrogen dioxide (NO2). This data is essential for monitoring environmental health and guiding policy decisions for air quality improvement.`,
      MinZoomLevel: 11,
      MarkersThreshold: -1,
      DisplayProperty: [
        { displayName: "Station Name", value: "station_name" },
        { displayName: "Nitrogen Dioxide (NO2)", value: "no2" },
        { displayName: "Particulate Matter (PM10)", value: "pm10" },
        { displayName: "Particulate Matter (PM2'5)", value: "pm2_5" },
      ],
      PolygonColoring: null,
      Tables: [],
    },
  },
];

// Iterate over datasets and insert only if DatasetId does not exist
datasets.forEach((dataset) => {
  if (
    !db.datasets.findOne({
      "basicData.DatasetId": dataset.basicData.DatasetId,
    })
  ) {
    db.datasets.insertOne(dataset);
  }
});
