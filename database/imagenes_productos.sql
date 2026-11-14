/* =============================================================================
   Imágenes de los productos de ejemplo (enlaces públicos).
   Se puede ejecutar varias veces: reemplaza la lista completa de cada producto.
   Formato de productos.imagenes: [{"url": "...", "orden": n, "principal": bool}]

   Fuentes y licencias
   - Imágenes oficiales del fabricante (Apple Newsroom, ASUS, Samsung, Kingston,
     TP-Link): material promocional público; © de cada marca.
   - Wikimedia Commons (licencias libres, requieren atribución):
       · Lenovo ThinkPad E14 G7 — Rakoon, CC0
         https://commons.wikimedia.org/wiki/File:Lenovo_ThinkPad_E14_G7_14_WUXGA.jpg
       · Kingston NV2 2TB — Jacek Halicki, CC BY-SA 4.0
         https://commons.wikimedia.org/wiki/File:2023_Dysk_SSD_Kingston_NV2_2TB.jpg
       · Logitech MX Master 3S (HS12, HS19) — Hayden Schiff, CC BY 4.0
         https://commons.wikimedia.org/wiki/File:Logitech_MX_Master_3S_HS12.jpg
         https://commons.wikimedia.org/wiki/File:Logitech_MX_Master_3S_HS19.jpg
   ========================================================================== */

UPDATE productos SET imagenes = '[
  {"url": "https://www.apple.com/newsroom/images/2024/03/apple-unveils-the-new-13-and-15-inch-macbook-air-with-the-powerful-m3-chip/tile/Apple-MacBook-Air-2-up-hero-240304-lp.jpg.og.jpg", "orden": 1, "principal": true}
]'::jsonb WHERE slug = 'macbook-air-13-m3';

UPDATE productos SET imagenes = '[
  {"url": "https://dlcdnwebimgs.asus.com/gain/14730014-9B03-4369-83EE-B668F348B5AA/w800", "orden": 1, "principal": true}
]'::jsonb WHERE slug = 'asus-rog-strix-g16';

UPDATE productos SET imagenes = '[
  {"url": "https://thumb.wikimedia.org/wikipedia/commons/thumb/c/c5/Lenovo_ThinkPad_E14_G7_14_WUXGA.jpg/960px-Lenovo_ThinkPad_E14_G7_14_WUXGA.jpg", "orden": 1, "principal": true}
]'::jsonb WHERE slug = 'thinkpad-e14';

UPDATE productos SET imagenes = '[
  {"url": "https://images.samsung.com/is/image/samsung/p6pim/ph/2401/gallery/ph-galaxy-s24-sm-s921bzkcphl-539300411?$650_519_PNG$", "orden": 1, "principal": true},
  {"url": "https://images.samsung.com/is/image/samsung/p6pim/ph/2401/gallery/ph-galaxy-s24-sm-s921bzacphl-539300298?$650_519_PNG$", "orden": 2, "principal": false}
]'::jsonb WHERE slug = 'galaxy-s24';

UPDATE productos SET imagenes = '[
  {"url": "https://media.kingston.com/kingston/product/FURY_Beast_Black_DDR5_1-lg.jpg", "orden": 1, "principal": true},
  {"url": "https://media.kingston.com/kingston/product/FURY_Beast_Black_DDR5_1_angle-lg.jpg", "orden": 2, "principal": false}
]'::jsonb WHERE slug = 'fury-beast-ddr5';

UPDATE productos SET imagenes = '[
  {"url": "https://thumb.wikimedia.org/wikipedia/commons/thumb/7/75/2023_Dysk_SSD_Kingston_NV2_2TB.jpg/960px-2023_Dysk_SSD_Kingston_NV2_2TB.jpg", "orden": 1, "principal": true}
]'::jsonb WHERE slug = 'kingston-nv2';

UPDATE productos SET imagenes = '[
  {"url": "https://thumb.wikimedia.org/wikipedia/commons/thumb/d/d4/Logitech_MX_Master_3S_HS12.jpg/960px-Logitech_MX_Master_3S_HS12.jpg", "orden": 1, "principal": true},
  {"url": "https://thumb.wikimedia.org/wikipedia/commons/thumb/e/e0/Logitech_MX_Master_3S_HS19.jpg/960px-Logitech_MX_Master_3S_HS19.jpg", "orden": 2, "principal": false}
]'::jsonb WHERE slug = 'mx-master-3s';

UPDATE productos SET imagenes = '[
  {"url": "https://static.tp-link.com/upload/image-line/Overview_Archer_AX55_01_large_20210816012555f.jpg", "orden": 1, "principal": true}
]'::jsonb WHERE slug = 'archer-ax55';

UPDATE productos SET imagenes = '[
  {"url": "https://images.samsung.com/is/image/samsung/p6pim/ph/lc27g55tqwexxp/gallery/ph-odyssey-g5-g55t-lc27g55tqwexxp-425475103?$650_519_PNG$", "orden": 1, "principal": true},
  {"url": "https://images.samsung.com/is/image/samsung/p6pim/ph/lc27g55tqwexxp/gallery/ph-odyssey-g5-g55t-lc27g55tqwexxp-425475104?$650_519_PNG$", "orden": 2, "principal": false},
  {"url": "https://images.samsung.com/is/image/samsung/p6pim/ph/lc27g55tqwexxp/gallery/ph-odyssey-g5-g55t-lc27g55tqwexxp-425475102?$650_519_PNG$", "orden": 3, "principal": false}
]'::jsonb WHERE slug = 'odyssey-g5-27';

/* Imagen por variante: el Galaxy S24 cambia de color según la versión */
UPDATE variantes_producto SET imagen_url = 'https://images.samsung.com/is/image/samsung/p6pim/ph/2401/gallery/ph-galaxy-s24-sm-s921bzkcphl-539300411?$650_519_PNG$'
 WHERE sku = 'S24-128-NEG';
UPDATE variantes_producto SET imagen_url = 'https://images.samsung.com/is/image/samsung/p6pim/ph/2401/gallery/ph-galaxy-s24-sm-s921bzacphl-539300298?$650_519_PNG$'
 WHERE sku = 'S24-256-GRIS';

/* Verificación */
SELECT p.slug, jsonb_array_length(p.imagenes) AS imagenes,
       (SELECT count(*) FROM variantes_producto v WHERE v.producto_id = p.id AND v.imagen_url IS NOT NULL) AS variantes_con_imagen
FROM productos p ORDER BY p.id;
