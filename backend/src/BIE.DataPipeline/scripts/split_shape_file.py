import geopandas as gpd
from shapely.geometry import box
import numpy as np

def create_grid(minx, miny, maxx, maxy, rows, cols):
    # Calculate width and height of each grid cell
    width = (maxx - minx) / cols
    height = (maxy - miny) / rows

    # Create grid cells
    grid_cells = []
    for i in range(cols):
        for j in range(rows):
            grid_cells.append(box(minx + i * width, miny + j * height,
                                  minx + (i + 1) * width, miny + (j + 1) * height))
    return grid_cells

def get_grid_index(geometry, grid_cells):
    centroid = geometry.centroid
    for i, grid_cell in enumerate(grid_cells):
        if grid_cell.contains(centroid):
            return i
    return -1

def split_shapefile(shapefile_path, rows, cols):
    # Load the shapefile
    print(f"Loading shapefile from {shapefile_path}...")
    gdf = gpd.read_file(shapefile_path)
    print("Shapefile loaded successfully.")

    # Get the bounds of the shapefile
    minx, miny, maxx, maxy = gdf.total_bounds
    print(f"Shapefile bounds: {minx}, {miny}, {maxx}, {maxy}")

    # Create grid
    grid_cells = create_grid(minx, miny, maxx, maxy, rows, cols)
    print(f"Grid with {rows} rows and {cols} columns created.")

    # Assign each shape to a grid cell based on its centroid
    print("Assigning shapes to grid cells based on centroids...")
    gdf['grid_index'] = gdf.geometry.apply(lambda geom: get_grid_index(geom, grid_cells))
    print("Shapes assigned to grid cells.")

    # Split the GeoDataFrame into parts based on the grid index
    num_parts = rows * cols
    gdfs = [gdf[gdf['grid_index'] == i] for i in range(num_parts)]
    print(f"GeoDataFrame split into {num_parts} parts.")

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

# Example usage
shapefile_path = 'hausumringe.shp'
rows = 2  # Number of horizontal divisions
cols = 2  # Number of vertical divisions
split_shapefile(shapefile_path, rows, cols)