import { forwardRef, useEffect, useImperativeHandle, useState } from "react";
import { MenuItemDto, UpsertMenuItemDto } from "./product";
import { productMenuService } from "../../../utils/services/product-menu.service";
import { useTranslation } from "react-i18next";
import EditAddMenuItemModal from "../management/menu/edit-add-menuItem-modal";
import Dropdown, { DropdownItem } from "../../../utils/components/dropdown";
import PaginationControls from "../../../utils/components/pagination-controlls";

const itemsPerPage = 20;
const menuTypes = [{id:'available' ,value:'Available'}, {id:'unavailable',value:'Unavailable'}, {id:'all',value:'/'}];
export const MenuTable = forwardRef(({ placeId }: { placeId: number }, ref) => {
  const [showModal, setShowModal] = useState(false);
  const [editingItemId, setEditingItemId] = useState<number | null>(null);
  const [editableProduct, setEditableProduct] = useState<Partial<MenuItemDto>>({});
  const [searchTerm, setSearchTerm] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [filteredMenu, setFilteredMenu] = useState<MenuItemDto[]>([]);
  const [totalItems, setTotalItems] = useState(0);
  const [menu, setMenu] = useState<any>([]);
  const [isAvailable, setIsAvailable] = useState<boolean |null>(true);
  const [isEditMode, setIsEditMode] = useState(true);
  const [availableProducts, setAvailableProducts] = useState<{ id: number; name: string }[]>([]);
  const { t } = useTranslation("admin");

  useImperativeHandle(ref, () => ({
    openAddModal: () => {
      setIsEditMode(false);
      setEditableProduct({ isAvailable: true });
      setShowModal(true);
    },
  }));

  const fetchMenu = async () => {
    if (!placeId) return;
    const results = await productMenuService.getMenuByPlaceId(placeId.toString(),false);
    setMenu(results);
    const allProducts = await productMenuService.getAllProducts();
    setAvailableProducts(allProducts);
  };

  useEffect(() => {
    filterMenuItems();
  }, [menu, searchTerm, currentPage, isAvailable]);

  const startEditing = (item: MenuItemDto) => {
    setEditingItemId(item.product.id);
    setEditableProduct({ ...item });
    setIsEditMode(true);
    setShowModal(true);
  };

  const cancelEdit = () => {
    setEditingItemId(null);
    setEditableProduct({});
  };

  const onChangeInput = (field: keyof MenuItemDto, value: any) => {
    setEditableProduct((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const saveChanges = async () => {
    if (!editableProduct) return;
    const updatedItem: UpsertMenuItemDto = {
      productId: (editableProduct.productId ?? editableProduct.product?.id)!,
      placeId: placeId,
      description: editableProduct.description ?? "",
      isAvailable: editableProduct.isAvailable ?? false,
      price: Number(editableProduct?.price) ?? 0,
    };
    
    
    try {
      if (isEditMode) {
        await productMenuService.updateMenuItem(updatedItem);
      } else {
        if(menu.find((el:any) => el.productId === editableProduct.product?.id))
          await productMenuService.updateMenuItem(updatedItem);
        else
          await productMenuService.saveProductsToPlace([updatedItem]);
      }
  
      await fetchMenu();
      setShowModal(false);
      cancelEdit();
    } catch (error) {
      console.error("Error saving item:", error);
    }
  };

  const filterMenuItems = () => {
    const search = searchTerm.toLowerCase();
    const filteredByAvailability = isAvailable ? menu.filter((item:MenuItemDto) => item.isAvailable) : isAvailable === false ? menu.filter((item:MenuItemDto) => item.isAvailable === false) : menu;
    const filtered = search
      ? filteredByAvailability.filter((item:any) => item.product.name.toLowerCase().includes(search))
      : filteredByAvailability;

    const paginated = filtered.slice(
      (currentPage - 1) * itemsPerPage,
      currentPage * itemsPerPage
    );
    setFilteredMenu(paginated);
    setTotalItems(filtered.length);
  };

  const changeMenuType = (item: DropdownItem) => {
    if (item.id === "available") {
      setIsAvailable(true);
    } else if (item.id === "unavailable") {
      setIsAvailable(false);
    } else {
      setIsAvailable(null); 
    }
    setCurrentPage(1);
  };
  useEffect(() => {
    fetchMenu();
  }, []);

  useEffect(() => {
    filterMenuItems();
  }, [menu, searchTerm, currentPage]);

  return (
    <section className="mt-4 bg-white p-4 sm:px-40 rounded-lg shadow col-span-1 md:col-span-2 overflow-x-auto">

      <div className="flex flex-col md:flex-row md:items-center  mt-2 mb-4 gap-2">
        <input
          type="text"
          placeholder={t("search")}
          className="border border-[#737373] rounded-[30px] text-black px-3 py-2 w-full md:w-1/3 "
          value={searchTerm}
          onChange={(e) => {
            setSearchTerm(e.target.value);
            setCurrentPage(1);
          }}
        />
        <Dropdown
          items={menuTypes}
          onChange={changeMenuType}
          type="custom"
          className="max-w-[180px] bg-white rounded-[16px]"
        />
      </div>

      <table className="min-w-[600px] w-full text-left text-sm text-black">
        <thead>
          <tr className="border-b-2 text-[#737373]">
            <th className="p-2 text-center">{t("product").toUpperCase()}</th>
            <th className="p-2 text-center">{t("description").toUpperCase()}</th>
            <th className="p-2 text-center">{t("price").toUpperCase()}</th>
            <th className="p-2 text-center">{t("availability").toUpperCase()}</th>
            <th className="p-2 text-center">{t("")}</th>
          </tr>
        </thead>
        <tbody>
          {filteredMenu.map((item,index) => {
            const isEditing = editingItemId === item.product.id;

            return (
              <tr
                key={item.product.id}
                className={`transition-all duration-200 ${
                  index%2 !== 0 ? "bg-[#F5F5F5]" : "bg-white"
                } `}
              >
                <td className="p-2 py-6 text-left">{item.product.name}</td>
                <td className={`p-2 ${item?.description ? "text-left" : "text-center"}`}>
                  {isEditing ? (
                    <input
                      type="text"
                      className="border rounded p-1 w-full"
                      value={editableProduct.description ?? ""}
                      onChange={(e) => onChangeInput("description", e.target.value)}
                    />
                  ) : (
                    item.description || "/"
                  )}
                </td>
                <td className="p-2 text-center">
                  {item.price}€
                </td>
                
                <td className="p-2 text-center">
                  <img className="m-auto" src="/assets/images/icons/checkboxCheckMark.svg" width="20px" />
                </td>
                <td className="p-2 flex gap-4 items-center text-center">
                  <button onClick={()=>{startEditing(item);}}><img src="/assets/images/icons/edit.svg" width="15px" /></button>
                  <button ><img src="/assets/images/icons/delete.png" width="15px" /></button>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>

      <PaginationControls
          currentPage={currentPage}
          totalItems={totalItems}
          itemsPerPage={itemsPerPage}
          onPageChange={setCurrentPage}
        />

      {showModal && (
        <EditAddMenuItemModal
          item={editableProduct}
          isEditMode={isEditMode}
          onClose={() => setShowModal(false)}
          onSave={() => {saveChanges();}}
          onChange={(field:any, value:any) => setEditableProduct((prev) => ({ ...prev, [field]: value }))}
          availableProducts={availableProducts}
        />
      )}
    </section>
  );
});
