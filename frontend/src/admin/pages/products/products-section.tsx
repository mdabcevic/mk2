import { forwardRef, useEffect, useImperativeHandle, useState } from "react";
import { productMenuService } from "../../../utils/services/product-menu.service";
import { Category, CreateCustomProductReq, MenuItem, Product } from "./product";
import { authService } from "../../../utils/auth/auth.service";
import { useTranslation } from "react-i18next";
import Dropdown, { DropdownItem } from "../../../utils/components/dropdown";
import AddProductModal from "./edit-add-product-modal";
import PaginationControls from "../../../utils/components/pagination-controlls";

const placeId = authService.placeId();
const filterOptions: DropdownItem[] = [
  { id: "custom", value: "Custom" },
  { id: "shared", value: "Shared" },
  { id: "all", value: "All" },
];

const ProductsSection = forwardRef((_, ref) => {
  const [products, setProducts] = useState<Product[]>([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterType, setFilterType] = useState<"custom" | "shared" | "all">("all");
  const [currentPage, setCurrentPage] = useState(1);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [categories, setCategories] = useState<Category[]>([]);
  const itemsPerPage = 30;
  const { t } = useTranslation("admin");

  const fetchCategories = async () => {
    const cats = await productMenuService.getProductCategories(placeId);
    setCategories(cats);
  };

  useEffect(() => {
    fetchProducts();
    fetchCategories();
  }, []);

  useImperativeHandle(ref, () => ({
    openAddModal() {
      setIsModalOpen(true);
    },
  }));

  const saveProduct = async (newProduct: CreateCustomProductReq) => {
    await productMenuService.createCustomProduct(newProduct);
    setIsModalOpen(false);
    const results = await productMenuService.getAllProducts(placeId);
    let maxId = 0;
    results.forEach(el => {if(el.id > maxId) maxId = el.id});
    const newMenuItem: MenuItem = {
      placeId:placeId,
      productId:maxId,
      price:0,
      isAvailable:true
    }
    await productMenuService.saveProductsToPlace([newMenuItem]);
    console.log(results)
  };

  const fetchProducts = async () => {
    const results = await productMenuService.getAllProducts(placeId);
    setProducts(results);
  };

  const dropdownChange = (item: DropdownItem) => {
    setFilterType(item.id as "custom" | "shared" | "all");
    setCurrentPage(1);
  };

  const filtered = products.filter((product) => {
    const matchesSearch = product.name.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesFilter =
      filterType === "all"
        ? true
        : filterType === "custom"
        ? product.exclusive
        : !product.exclusive;
    return matchesSearch && matchesFilter;
  });

  const totalItems = filtered.length;
  const totalPages = Math.ceil(totalItems / itemsPerPage);
  const paginatedProducts = filtered.slice(
    (currentPage - 1) * itemsPerPage,
    currentPage * itemsPerPage
  );

  return (
    <div className="p-4 w-full max-w-[1500px] bg-gray-50 text-gray-800 m-auto">
      <section className="bg-white p-4 w-full">
        <div className="flex flex-col md:flex-row md:items-center gap-2 mb-4">
          <input
            type="text"
            placeholder={t("search")}
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              setCurrentPage(1);
            }}
            className="border border-gray-300 rounded-[30px] px-3 py-2 w-full md:w-1/3"
          />
          <Dropdown
            items={filterOptions}
            onChange={dropdownChange}
            type="custom"
            className="max-w-[180px] bg-white rounded-[16px]"
          />
        </div>

        <table className="min-w-[600px] w-full text-left text-sm text-black">
          <thead>
            <tr className="border-b-2 text-[#737373]">
              <th className="p-2 text-center">{t("product").toUpperCase()}</th>
              <th className="p-2 text-center">{t("volume").toUpperCase()}</th>
              <th className="p-2 text-center">{t("category").toUpperCase()}</th>
              <th className="p-2 text-center">{t("custom").toUpperCase()}</th>
              <th className="p-2 text-center"></th>
            </tr>
          </thead>
          <tbody>
            {paginatedProducts.map((item, index) => (
              <tr
                key={item.id}
                className={`transition-all duration-200 ${index % 2 !== 0 ? "bg-[#F5F5F5]" : "bg-white"}`}
              >
                <td className="py-6 px-2 text-left">{item.name}</td>
                <td className="py-6 px-2 text-center">{item.volume}</td>
                <td className="py-6 px-2 text-center">{item.category.name}</td>
                <td className="py-6 px-2 text-center">
                  {item.exclusive ? (
                    <img src="/assets/images/icons/checkboxCheckMark.svg" width="20px" className="m-auto" />
                  ) : (
                    <span className="block m-auto">No</span>
                  )}
                </td>
                <td className="py-6 px-2 flex gap-4 items-center justify-center">
                  <button><img src="/assets/images/icons/delete.png" width="15px" /></button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        <PaginationControls
          currentPage={currentPage}
          totalItems={products.length}
          itemsPerPage={itemsPerPage}
          onPageChange={setCurrentPage}
        />
      </section>


      <AddProductModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSave={saveProduct}
        categories={categories}
      />
    </div>
  );
});

export default ProductsSection;
