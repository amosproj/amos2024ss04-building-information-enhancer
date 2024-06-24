import geopandas as gpd
from shapely.geometry import box

# Load the shapefile
shapefile_path = 'hausumringe.shp'
print(f"Loading shapefile from {shapefile_path}...")
gdf = gpd.read_file(shapefile_path)
print("Shapefile loaded successfully.")

# Get the bounds of the shapefile
minx, miny, maxx, maxy = gdf.total_bounds
print(f"Shapefile bounds: {minx}, {miny}, {maxx}, {maxy}")

# Calculate the midpoint coordinates
midx = (minx + maxx) / 2
midy = (miny + maxy) / 2
print(f"Midpoint coordinates: {midx}, {midy}")

# Define the four quadrants
quadrants = [
    box(minx, midy, midx, maxy),  # Top-left
    box(midx, midy, maxx, maxy),  # Top-right
    box(minx, miny, midx, midy),  # Bottom-left
    box(midx, miny, maxx, midy)   # Bottom-right
]
print("Quadrants defined.")

# Function to determine the quadrant of a shape based on its centroid
def get_quadrant(geometry, quadrants):
    centroid = geometry.centroid
    for i, quadrant in enumerate(quadrants):
        if quadrant.contains(centroid):
            return i
    return -1

# Assign each shape to a quadrant based on its centroid
print("Assigning shapes to quadrants based on centroids...")
gdf['quadrant'] = gdf.geometry.apply(lambda geom: get_quadrant(geom, quadrants))
print("Shapes assigned to quadrants.")

# Split the GeoDataFrame into four parts based on the quadrant
gdfs = [gdf[gdf['quadrant'] == i] for i in range(4)]
print("GeoDataFrame split into four parts.")

# Save each part as a new shapefile
for i, gdf_part in enumerate(gdfs):
    if not gdf_part.empty:
        output_path = f'hausumringe_mittelfranken_{i+1}.shp'
        print(f"Saving part {i+1} to {output_path}...")
        gdf_part.to_file(output_path)
        print(f"Part {i+1} saved successfully.")
    else:
        print(f"Part {i+1} is empty and was not saved.")

print("All parts processed and saved.")