# Instructions Copilot - Nglib

Il s'agit d'un ensemble de bibliothèques .Net réutilisables et OpenSources.
Disponibles sur Nuget et Github.

## Règles de développement  
Toutes les règles de développement, conventions et bonnes pratiques pour ce projet sont documentées dans le fichier `docs/wiki_dev.md`.
Tu as l'**obligation** de les suivre avant toute modification du code.

Règles obligatoires :
- Toujours attendre ma validation avant de modifier le code ou créer de nouveaux fichiers.
- Toujours créer un test unitaire pour valider les modifications et documenter les changements.
- Toute la documentation et les commentaires doivent être en anglais.
- Reste simple, fais uniquement ce qui est demandé, n'en fais pas plus.
- Pour les changements significatifs, proposer une mise à jour du `CHANGELOG.md`.

**Règle Absolue** :  Proposer des solutions et Toujours attendre une validation explicite avant de modifier le code. TOUS DOIT ETRE APPROUVÉ par l'utilisateur !

## Structure du projet  
Ce projet est une solution .NET avec plusieurs composants modulaires situés dans le dossier `dev/`.
La documentation du projet est située dans le dossier `docs/`.
Tu as la possibilité de créer des fichiers temporaires dans le dossier `tmp/` pour tester des idées ou des concepts, mais ces fichiers ne doivent pas être committés.
Les documents qui commencent par wiki_* sont des documents officiels de l'équipe de développement.