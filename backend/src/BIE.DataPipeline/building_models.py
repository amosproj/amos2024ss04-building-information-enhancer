"""
This scripts generates all yaml files in a given meta4 file and saves them in a folder named building_models_yaml_file.
The path to the meta4 file has to be set in this script.
"""
import xml.etree.ElementTree as ET
#pip install pyyaml
import yaml
import os
file_path = 'This should be the full path the the meta4 file eg: C:\\folder\\test.meta4'

def extract_file_info(xml_data):
    # Parse the XML data
    root = ET.fromstring(xml_data)

    # Define the namespace
    ns = {'ml': 'urn:ietf:params:xml:ns:metalink'}

    # Extract file name and the first URL for each file
    file_info = []
    for file in root.findall('ml:file', ns):
        name = file.get('name')
        url = file.find('ml:url', ns).text
        file_info.append((name, url))

    return file_info

def create_folder(path):
    try:
        # Create the folder
        os.makedirs(path, exist_ok=True)
        print(f"Folder created successfully at: {path}")
    except Exception as e:
        print(f"An error occurred: {e}")

def create_yaml_file(file_path, name, url):
    table_name = "building_models_" + name
    
    # Data to be written to the JSON file
    data = {
        "source": {
            "type": "URL",
            "location": url,
            "data_format": "CITYGML"
        },
        "options": {
            "skip_lines": 0,
            "discard_null_rows": False,
            "if_table_exists": "skip"
        },
        "table_name": table_name,
        "table_cols": [],
        "dataset": "building_models"
    }
    
    target_file = file_path + "\\" + table_name + ".yaml"

    # Write data to a JSON file
    with open(target_file, 'w', encoding='utf-8') as json_file:
        yaml.dump(data, json_file, indent=4)

def get_script_location():
    # Get the absolute path of the current script
    script_path = os.path.abspath(__file__)
    # Get the directory containing the script
    script_dir = os.path.dirname(script_path)
    return script_path, script_dir

def remove_file_extension(file_name):
    # Split the file name into a name and extension
    name, _ = os.path.splitext(file_name)
    return name

# Get the location of the current script
script_path, script_dir = get_script_location()


target_folder_path = script_dir + "\\building_models_yaml_file"
with open(file_path, 'r', encoding='utf-8') as file:
        xml_data = file.read()
file_info = extract_file_info(xml_data)
full_path = os.path.abspath(target_folder_path)
print(f"Full path: {full_path}")
create_folder(target_folder_path)
i = 0
for name, url in file_info:
    i = i + 1
    print(i)
    #print(f"File Name: {name}\nFirst URL: {url}\n")
    create_yaml_file(target_folder_path, remove_file_extension(name), url)