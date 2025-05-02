
export default function EditAddMenuItemModal({
    item,
    isEditMode,
    onClose,
    onSave,
    onChange,
    availableProducts,
  }: {
    item: any;
    isEditMode: boolean;
    onClose: () => void;
    onSave: () => void;
    onChange: (field: string, value: any) => void;
    availableProducts: { id: number; name: string }[];
  }) {

    return (
      <div className="fixed inset-0 flex items-center justify-center bg-black/10 z-50 bg-opacity-10">
        <div className="bg-white rounded-xl p-6 w-[400px] shadow-xl">
          <div className="flex justify-between w-full">
            <h2 className="text-lg text-[#A3A3A3] font-semibold mb-4">{isEditMode ? "Edit Menu Item" : "Add Menu Item"}</h2>
            <button onClick={onClose}><img src="/assets/images/close.svg" width="30px" /></button>
          </div>
          
          <div className="mb-4">
            <label className="block mb-1 font-medium">Product</label>
            {!isEditMode ? (
                <select
                value={item.productId ?? item.product?.id ?? ""}
                onChange={(e) => onChange("productId", Number(e.target.value))}
                className="w-full border rounded-[16px] p-2"
              >
                <option value="" disabled>Select product</option>
                {availableProducts.map((p) => (
                  <option key={p.id} value={p.id}>{p.name}</option>
                ))}
              </select>
            ) : <p>{item.product.name}</p>}
            
          </div>

          <div className="mb-4">
            <label className="block mb-1  font-medium">Description</label>
            <textarea
              className="w-full border rounded-[16px] p-2"
              rows={3}
              value={item.description ?? ""}
              onChange={(e) => onChange("description", e.target.value)}
            />
          </div>
  
          <div className="mb-4">
            <label className="block mb-1 font-medium">Price (€)</label>
            <input
              type="number"
              className="w-full border rounded-[16px] p-2"
              value={item.price ?? ""}
              onChange={(e) => onChange("price", parseFloat(e.target.value))}
            />
          </div>
  
          <div className="mb-4 flex items-center gap-2">
            <input
              type="checkbox"
              checked={item.isAvailable ?? false}
              onChange={(e) => onChange("isAvailable", e.target.checked)}
            />
            <label className="font-medium">Available</label>
          </div>
  
          <div className="flex justify-end gap-2">
            <button
              onClick={onSave}
              className="px-4 py-2 rounded-[16px] text-white"
              style={{ backgroundColor: "#624935" }}
            >
              Save
            </button>
          </div>
        </div>
      </div>
    );
  }
  