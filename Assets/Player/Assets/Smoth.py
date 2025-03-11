from PIL import Image
import numpy as np
import sys

# Charger l'image en noir et blanc
image_path = sys.argv[1]
image = Image.open(image_path).convert('L')  # Convertir en niveaux de gris

# Convertir l'image en tableau numpy
image_array = np.array(image)

# Normaliser les valeurs de l'image pour qu'elles soient entre 0 et 1
normalized_image = image_array / 255.0

# Créer un tableau RGBA (avec alpha) à partir de l'image
rgba_image = np.dstack((image_array, image_array, image_array, normalized_image))

# Convertir le tableau RGBA en image PIL
rgba_image_pil = Image.fromarray((rgba_image * 255).astype(np.uint8))

# Sauvegarder l'image résultante
rgba_image_pil.save('image_with_alpha.png')

# Afficher l'image résultante
rgba_image_pil.show()
