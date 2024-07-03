import xml.etree.ElementTree as ET

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

file_path = 'C:\\Users\\nicol\\Downloads\\09562.meta4'
with open(file_path, 'r', encoding='utf-8') as file:
        xml_data = file.read()
file_info = extract_file_info(xml_data)
for name, url in file_info:
    print(f"File Name: {name}\nFirst URL: {url}\n")