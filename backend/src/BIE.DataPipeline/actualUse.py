import os

def create_yaml_files(input_file, output_dir):
    
    with open(input_file, 'r', encoding="utf-8") as infile:
        for i, line in enumerate(infile):
            parts = line.strip().split(",")

            link = parts[0]
            name = parts[1].replace(" ", "_")

            name = name.replace(u'.', "_")
            name = name.replace(u'-', "_")
            name = name.replace(u'ä', "ae")
            name = name.replace(u'Ä', "Ae")
            name = name.replace(u'ö', "oe")
            name = name.replace(u'Ö', "Oe")
            name = name.replace(u'ü', "ue")
            name = name.replace(u'Ü', "Ue")
            name = name.replace(u'ß', "ss")
            name = name.replace(u'(', "")
            name = name.replace(u')', "")
            
            
            # Generate yaml file contents
            file_content = f"""# describe the source
source:
  # link | filepath
  type: URL
  location: {link}
  data_format: SHAPE
options:
  # skip lines at the beginning
  skip_lines: 0
  # discard any rows that have null values
  discard_null_rows: false
  # how to deal with existing table. Options: ignore, replace, skip (default).
  if_table_exists: skip
table_name: actual_use_{name}

table_cols:

"""
            
            output_file = os.path.join(output_dir, f'actual_use_{name}.yaml')
            
            with open(output_file, 'w') as outfile:
                outfile.write(file_content)
                
            print(f'Created: {output_file}')


input_file = 'actual_use_datasets'
output_dir = 'yaml'

create_yaml_files(input_file, output_dir)