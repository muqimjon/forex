-- =============================================================================
-- Forex — Barkod tizimi (ProductType barkodlari + to'plam + qaytarish restock)
-- =============================================================================
-- ProductTypes:
--   "QopBarcode"    — qop (sack) barkodi (noyob, NULL bo'lishi mumkin)
--   "PackBarcode"   — to'plam (pack) barkodi (noyob, NULL bo'lishi mumkin)
--   "PackItemCount" — 1 to'plamdagi juftlar soni (5/6)
--
-- ReturnItems:
--   "RestockCount"    — qaytgan mahsulotning FAQAT qopda kelgan, omborga qaytadigan
--                       juftlar soni. To'plam/dona qaytsa faqat narxi hisoblanadi
--                       (kredit), omborga qaytmaydi (qadoqlash bo'limiga olinadi).
--                       Mavjud (eski) qaytarishlar to'liq "TotalCount" bilan
--                       omborga kirgan edi — shuning uchun ular uchun
--                       "RestockCount" = "TotalCount" qilib to'ldiriladi
--                       (tahrirlashda ombor qoldig'i to'g'ri tiklanishi uchun).
--
-- Ishlatish: bir marta ishga tushiring (idempotent — qayta ishga tushsa xavfsiz).
-- Mavjud turlarga barkodni dasturdagi "Barcha barkodlarni yaratish" amali to'ldiradi.
-- =============================================================================

BEGIN;

ALTER TABLE "ProductTypes" ADD COLUMN IF NOT EXISTS "QopBarcode" text;
ALTER TABLE "ProductTypes" ADD COLUMN IF NOT EXISTS "PackBarcode" text;
ALTER TABLE "ProductTypes" ADD COLUMN IF NOT EXISTS "PackItemCount" integer NOT NULL DEFAULT 0;

DROP INDEX IF EXISTS "IX_ProductTypes_QopBarcode";
CREATE UNIQUE INDEX "IX_ProductTypes_QopBarcode"
    ON "ProductTypes" ("QopBarcode") WHERE "QopBarcode" IS NOT NULL AND "QopBarcode" <> '';

DROP INDEX IF EXISTS "IX_ProductTypes_PackBarcode";
CREATE UNIQUE INDEX "IX_ProductTypes_PackBarcode"
    ON "ProductTypes" ("PackBarcode") WHERE "PackBarcode" IS NOT NULL AND "PackBarcode" <> '';

ALTER TABLE "ReturnItems" ADD COLUMN IF NOT EXISTS "RestockCount" integer;
UPDATE "ReturnItems" SET "RestockCount" = "TotalCount" WHERE "RestockCount" IS NULL;
ALTER TABLE "ReturnItems" ALTER COLUMN "RestockCount" SET DEFAULT 0;
ALTER TABLE "ReturnItems" ALTER COLUMN "RestockCount" SET NOT NULL;

-- =============================================================================
-- Rol / bo'lim ruxsatlari tizimi (hodimlar uchun)
-- =============================================================================
-- Users:
--   "AccessMask" — login qila oladigan hodimga berilgan bo'lim ruxsatlari bitmask'i
--                  (AccessPermissions [Flags]). Har bit bitta menyu bo'limiga mos:
--                    Sales=1, Returns=2, Payments=4, Products=8, Barcode=16,
--                    Supply=32, Users=64, Reports=128, Settings=256; All=511.
--                  0 = hech qanday bo'lim. 'admin' username dasturda doim All hisoblanadi.
-- =============================================================================

ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "AccessMask" bigint NOT NULL DEFAULT 0;

-- Mavjud admin barcha bo'limlarga ega bo'lsin (511 = All).
UPDATE "Users" SET "AccessMask" = 511 WHERE lower("Username") = 'admin';

COMMIT;
