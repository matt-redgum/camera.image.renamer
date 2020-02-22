# camera.image.renamer

Simple renaming tool for generic image file names

Renames files in a path that match the filter to be the folder name plus a unique code

eg: 
```
C:\Photos\MyBirthday-2018\IMG_0233.JPG
C:\Photos\MyBirthday-2018\IMG_0234.JPG
C:\Photos\MyBirthday-2018\IMG_0235.MOV
```
gets renamed to
```
C:\Photos\MyBirthday-2018\MyBirthday-2018-5gK23s.JPG
C:\Photos\MyBirthday-2018\MyBirthday-2018-1kaa49.JPG
C:\Photos\MyBirthday-2018\MyBirthday-2018-8874wb.MOV
```

## Usage

`dotnet camera.image.renamer.dll [options] {filepath}`

## Command Line Options

`--filters` or `-f` : comma separated set of file filters. Defaults to standard file masks, but can be regular expressions with the use of the `--regex-filters` option  
If no filters are provided, the following default filters are used:
```
IMG_*.JPG
IMG_*.MOV
DSC_*.JPG
{Guid}.JPG
```

`--regex-filters` or `-x` : Indicates that the provided filters are regex filters rather than standard file masks

`--copyandrename` or `-c` : Copies the files in place with the new name, leaving the original file as-is.

`--recurse` or `-r` : Recurse the sub-directories below the file path

## Examples

Copy files to a new name, processing the provided directory and all sub directories

`dotnet camera.image.renamer.dll --recurse --copyandrename C:\path\to\photos\`

Rename only files that match the mask `IMG_*.JPG`, processing all subdirectories

`dotnet camera.image.renamer.dll --recurse --filters IMG_*.JPG C:\path\to\photos\`

Test process the folder to see what would be renamed, but don't actually rename the file. Only files that match the mask `IMG_*.JPG` should be processed, and process all subdirectories

`dotnet camera.image.renamer.dll --recurse --filters IMG_*.JPG --test C:\path\to\photos\`