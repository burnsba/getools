# Model

This currently only contains preliminary support for model data.

Model bounding box and scale definitions are loaded from (embedded) json files. Hopefully one day this will allow importing model definitions outside the basic game data.

These files are:

- ModelBboxDefinition.json: contains model name and bounding box info.
- ModelScaleDefinition.json: contains model name and scale.

The above json files were created from a simple program that can be found in the `/tools/GeModelBboxDump` folder.
