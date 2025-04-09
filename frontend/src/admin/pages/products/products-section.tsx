import { useEffect, useState } from "react";
import { Trash2, CheckCircle, XCircle } from "lucide-react";
import { productMenuService } from "../../../utils/services/product-menu.service";
import { Category, Product, MenuItem } from "./product";
import { authService } from "../../../utils/auth/auth.service";
import { useTranslation } from "react-i18next";


const placeId = authService.placeId();

function ProductsSection() {
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [selectedProducts, setSelectedProducts] = useState<Record<number, Product[]>>({});
  const [expandedCategory, setExpandedCategory] = useState<number | null>(null);
  const { t } = useTranslation("admin");  

  useEffect(() => {
    fetchProductCategories();
    fetchProducts();
  }, []);

  const fetchProducts = async () => {
    const results = await productMenuService.getAllProducts(placeId);
    setProducts(results);
    setSelectedProducts({});
  };

  const fetchProductCategories = async () => {
    const results = await productMenuService.getProductCategories(placeId);
    setCategories(results);
  };

  const toggleProductSelection = (categoryId: number, product: Product) => {
    setSelectedProducts((prev) => {
      const newSelection = { ...prev };
      if (!newSelection[categoryId]) newSelection[categoryId] = [];

      if (newSelection[categoryId].some((p) => p.id === product.id)) {
        newSelection[categoryId] = newSelection[categoryId].filter((p) => p.id !== product.id);
      } else {
        newSelection[categoryId] = [...newSelection[categoryId], product];
      }

      return newSelection;
    });
  };

  const removeProduct = (categoryId: number, product: Product) => {
    toggleProductSelection(categoryId, product);
  };

  const saveChanges = async () => {
    const productIds = Object.values(selectedProducts)
      .flat()
      .map((product) => product.id);

    const createMenuRequest: MenuItem[] = productIds.map((productId) => ({
      productId,
      placeId,
      isAvailable: true,
      price: 0,
      description: null,
    }));

    const response = await productMenuService.saveProductsToPlace(createMenuRequest);
  };

  return (
    
    <div className="p-4 grid grid-cols-1 md:grid-cols-2 gap-4 bg-gray-50 text-gray-800 rounded-lg shadow-md">

      <section className="bg-white p-4 rounded-lg shadow">
        <h2 className="text-lg font-semibold">{t("available_products")}</h2>
        {categories.map((category) => (
          <div key={category.id} className="mb-4">
            <h3
              className="text-md font-semibold cursor-pointer bg-gray-200 p-2 rounded-lg"
              onClick={() =>
                setExpandedCategory(expandedCategory === category.id ? null : category.id)
              }
            >
              {category.name}
            </h3>
            {expandedCategory === category.id && (
              <div className="mt-2 p-2 bg-gray-100 rounded-lg">
                {products
                  .filter((product) => product.category.id === category.id)
                  .map((product) => (
                    <div key={product.id} className="flex items-center gap-2 p-1">
                      <input
                        type="checkbox"
                        checked={
                          selectedProducts[category.id]?.some((p) => p.id === product.id) || false
                        }
                        onChange={() => toggleProductSelection(category.id, product)}
                      />
                      {product.name} ({product.volume})
                    </div>
                  ))}
              </div>
            )}
          </div>
        ))}
      </section>

      <section className="bg-white p-4 rounded-lg shadow">
        <h2 className="text-lg font-semibold">{t("selected_products")}</h2>
        {Object.entries(selectedProducts).map(([categoryId, products]) =>
          products.length > 0 ? (
            <div key={categoryId} className="mb-4">
              <h3 className="text-md font-semibold bg-gray-200 p-2 rounded-lg">
                {categories.find((cat) => cat.id === Number(categoryId))?.name}
              </h3>
              <ul>
                {products.map((product) => (
                  <li key={product.id} className="flex items-center gap-2 p-1">
                    {product.name} ({product.volume})
                    <button
                      onClick={() => removeProduct(Number(categoryId), product)}
                      className="text-red-500"
                    >
                      <Trash2 size={16} />
                    </button>
                    <button className="text-green-500">
                      <CheckCircle size={16} />
                    </button>
                    <button className="text-gray-500">
                      <XCircle size={16} />
                    </button>
                  </li>
                ))}
              </ul>
            </div>
          ) : null
        )}
      </section>

      <button
        onClick={saveChanges}
        className="mt-4 col-span-1 md:col-span-2 max-w-[120px] text-white py-2 rounded-lg bg-[#f49241] cursor-pointer"
      >
        {t("save")}
      </button>

    </div>
  );
}

export default ProductsSection;
