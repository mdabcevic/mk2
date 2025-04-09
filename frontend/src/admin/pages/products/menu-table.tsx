import { useEffect, useState } from "react";
import { MenuItemDto, UpsertMenuItemDto } from "./product";
import { productMenuService } from "../../../utils/services/product-menu.service";
import { Pencil, Save, ToggleLeft, ToggleRight } from "lucide-react";
import { useTranslation } from "react-i18next";

const itemsPerPage = 20;

function MenuTable({ placeId }: { placeId: number }) {
  const [editingItemId, setEditingItemId] = useState<number | null>(null);
  const [editableProduct, setEditableProduct] = useState<Partial<MenuItemDto>>({});
  const [searchTerm, setSearchTerm] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [filteredMenu, setFilteredMenu] = useState<MenuItemDto[]>([]);
  const [totalItems, setTotalItems] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [menu, setMenu] = useState<any>([]);
  const { t } = useTranslation("admin");

  const fetchMenu = async () => {
    if (!placeId) return;
    const results = await productMenuService.getMenuByPlaceId(placeId.toString(),false);
    setMenu(results);
  };

  const startEditing = (item: MenuItemDto) => {
    setEditingItemId(item.product.id);
    setEditableProduct({ ...item });
  };

  const cancelEdit = () => {
    setEditingItemId(null);
    setEditableProduct({});
  };

  const handleChange = (field: keyof MenuItemDto, value: any) => {
    setEditableProduct((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const toggleAvailability = () => {
    setEditableProduct((prev) => ({
      ...prev,
      isAvailable: !prev.isAvailable,
    }));
  };

  const saveChanges = async () => {
    if (!editableProduct) return;
    console.log(editableProduct);
    const updatedItem: UpsertMenuItemDto = {} as UpsertMenuItemDto;
    updatedItem.productId = editableProduct!.product!.id;
    updatedItem.placeId = placeId;
    updatedItem.description = editableProduct.description ?? "";
    updatedItem.isAvailable = editableProduct.isAvailable ?? false;
    updatedItem.price = Number(editableProduct?.price) ?? 0;

    try {
      let res = await productMenuService.updateMenuItem(updatedItem);
      console.log(res)
      await fetchMenu();
      cancelEdit();
    } catch (error) {
      console.error("Error updating item:", error);
    }
  };

  const hasChanges = (item: MenuItemDto) => {
    return (
      editableProduct.price !== item.price ||
      editableProduct.description !== item.description ||
      editableProduct.isAvailable !== item.isAvailable
    );
  };

  const filterMenuItems = () => {
    const search = searchTerm.toLowerCase();
    const filtered = search
      ? menu.filter((item) => item.product.name.toLowerCase().includes(search))
      : menu;

    const total = Math.ceil(filtered.length / itemsPerPage);
    const paginated = filtered.slice(
      (currentPage - 1) * itemsPerPage,
      currentPage * itemsPerPage
    );
    setFilteredMenu(paginated);
    setTotalItems(filtered.length);
    setTotalPages(total);
  };

  useEffect(() => {
    fetchMenu();
  }, []);

  useEffect(() => {
    filterMenuItems();
  }, [menu, searchTerm, currentPage]);

  return (
    <section className="mt-4 bg-white p-4 rounded-lg shadow col-span-1 md:col-span-2 overflow-x-auto">
      <h2 className="text-lg font-semibold mb-4">{t("my_menu")}</h2>

      <div className="flex flex-col md:flex-row md:items-center justify-between mb-4 gap-2">
        <input
          type="text"
          placeholder={t("search")}
          className="border border-gray-300 rounded text-black px-3 py-1 w-full md:w-1/3"
          value={searchTerm}
          onChange={(e) => {
            setSearchTerm(e.target.value);
            setCurrentPage(1);
          }}
        />
      </div>

      <table className="min-w-[600px] w-full text-left text-sm text-black">
        <thead>
          <tr className="bg-gray-100">
            <th className="p-2">{t("product")}</th>
            <th className="p-2">{t("volume")}</th>
            <th className="p-2">{t("price")}</th>
            <th className="p-2">{t("description")}</th>
            <th className="p-2">{t("availability")}</th>
            <th className="p-2">{t("")}</th>
          </tr>
        </thead>
        <tbody>
          {filteredMenu.map((item) => {
            const isEditing = editingItemId === item.product.id;

            return (
              <tr
                key={item.product.id}
                className={`border-b transition-all duration-200 ${
                  isEditing ? "bg-yellow-50" : "hover:bg-gray-50"
                }`}
              >
                <td className="p-2">{item.product.name}</td>
                <td className="p-2">{item.product.volume}</td>
                <td className="p-2">
                  {isEditing ? (
                    <input
                      type="number"
                      className="border rounded p-1 w-20"
                      value={editableProduct.price ?? ""}
                      onChange={(e) => handleChange("price", parseFloat(e.target.value))}
                    />
                  ) : (
                    item.price
                  )}
                </td>
                <td className="p-2">
                  {isEditing ? (
                    <input
                      type="text"
                      className="border rounded p-1 w-full"
                      value={editableProduct.description ?? ""}
                      onChange={(e) => handleChange("description", e.target.value)}
                    />
                  ) : (
                    item.description || "-"
                  )}
                </td>
                <td className="p-2">
                  {isEditing ? (
                    <button onClick={toggleAvailability}>
                      {editableProduct.isAvailable ? (
                        <ToggleRight size={20} className="text-green-500" />
                      ) : (
                        <ToggleLeft size={20} className="text-gray-400" />
                      )}
                    </button>
                  ) : item.isAvailable ? (
                    <ToggleRight size={20} className="text-green-500 opacity-50" />
                  ) : (
                    <ToggleLeft size={20} className="text-gray-400 opacity-50" />
                  )}
                </td>
                <td className="p-2 flex gap-2 items-center">
                  {isEditing ? (
                    <>
                      <button
                        onClick={saveChanges}
                        className={`text-green-500 ${
                          hasChanges(item)
                            ? "opacity-100"
                            : "opacity-50 cursor-not-allowed"
                        }`}
                        disabled={!hasChanges(item)}
                      >
                        <Save size={16} />
                      </button>
                      <button onClick={cancelEdit} className="text-red-500">
                        ✕
                      </button>
                    </>
                  ) : editingItemId === null ? (
                    <button onClick={() => startEditing(item)} className="text-blue-500">
                      <Pencil size={16} />
                    </button>
                  ) : null}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>

      <div className="flex justify-between items-center mt-4">
        <p className="text-sm text-gray-600">
          {(currentPage - 1) * itemsPerPage + 1}–{Math.min(currentPage * itemsPerPage, totalItems)} {t("of")} {totalItems}
        </p>

        <div className="flex gap-2">
          <button
            onClick={() => setCurrentPage((prev) => Math.max(prev - 1, 1))}
            disabled={currentPage === 1}
            className="px-3 py-1 border rounded disabled:opacity-50"
          >
            {t("previous_page")}
          </button>
          <span className="px-3 py-1 text-sm">
            {currentPage} / {totalPages}
          </span>
          <button
            onClick={() => setCurrentPage((prev) => Math.min(prev + 1, totalPages))}
            disabled={currentPage === totalPages}
            className="px-3 py-1 border rounded disabled:opacity-50"
          >
            {t("next_page")}
          </button>
        </div>
      </div>
    </section>
  );
}

export default MenuTable;
