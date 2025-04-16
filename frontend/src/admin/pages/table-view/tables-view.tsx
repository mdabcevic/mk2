import { useState, useRef, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Rnd } from "react-rnd";
import { tableService } from "../../../utils/services/tables.service";
import { BtnVisibility, Table, TableColor, TableStatus } from "../../../utils/constants";



const screenWidth = window.innerWidth;
const minScreenSize = 850;

function TableViewPage() {
  const containerRef = useRef<HTMLDivElement>(null);
  const { t } = useTranslation("admin");

  const [tables, setTables] = useState<Table[]>([]);
  const [newTableName, setNewTableName] = useState<string>("");
  const [newSeats, setNewSeats] = useState<number>(4);
  const [labelMessage, setLabelMessage] = useState<string>("");
  const [btnVisibility, setBtnVisibility] = useState<string>(BtnVisibility.invisible);
  const [selectedTableIndex, setSelectedTableIndex] = useState<number | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [mouseDownPosition, setMouseDownPosition] = useState<{ x: number; y: number } | null>(null);
  const addTable = () => {
    if (!newTableName.trim()) return;

    if (tables.find((table) => table.label === newTableName)) {
      setLabelMessage(t("table_already_exist"));
      return;
    }

    const newTable: Table = {
      label: newTableName,
      positionX: 0,
      positionY: 0,
      width: 100,
      height: 100,
      seats: newSeats,
      status: TableStatus.empty,
    };

    setTables((prev) => [...prev, newTable]);
    setNewTableName("");
    setNewSeats(4);
    setLabelMessage("");
    setBtnVisibility(BtnVisibility.visible);
  };

  const updateTable = (index: number, position: any, size?: any) => {
    setTables((prevTables) => {
      const updated = [...prevTables];
      updated[index] = {
        ...updated[index],
        positionX: position.x,
        positionY: position.y,
        ...(size && {
          width: size.width,
          height: size.height,
        }),
      };
      return updated;
    });
  };

  const isSignificantDrag = (start: { x: number; y: number }, end: { x: number; y: number }) => {
    const dx = Math.abs(end.x - start.x);
    const dy = Math.abs(end.y - start.y);
    return dx > 5 || dy > 5;
  };

  const savePlaceGround = async () => {
    // await tableService.savePlaceTables(tables);
    setBtnVisibility(BtnVisibility.invisible);
  };

  const deleteTable = (index: number) => {
    setTables((prev) => prev.filter((_, i) => i !== index));
    setSelectedTableIndex(null);
  };

  const fetchTables = async () => {
    const response = await tableService.getPlaceTablesByCurrent();
    setTables(response);
  };

  const getTableColor = (status: number) => {
    switch (status) {
      case TableStatus.occupied:
        return TableColor.occupied;
      case TableStatus.reserved:
        return TableColor.reserved;
      default:
        return TableColor.empty;
    }
  };

  useEffect(() => {
    fetchTables();
  }, []);

  return (
    <div className="flex flex-col items-center p-4 space-y-6 mt-20">
      {screenWidth >= minScreenSize && (
        <>
          <div className="flex flex-col w-full" id="table-info">
            <h2 className="text-base font-semibold text-left text-brown-500">{t("table_info")}</h2>
            <div className="flex flex-row items-center">
              <label className="text-brown-500">{t("table_label")}:</label>
              <div className="flex flex-col ml-2 mr-4">
                <input
                  type="text"
                  value={newTableName}
                  onChange={(e) => setNewTableName(e.target.value)}
                  placeholder="753-23"
                  className="p-1 border rounded text-sm text-brown-500"
                />
                <span className="text-red-500">{labelMessage}</span>
              </div>

              <label className="text-brown-500">{t("seats_number")}:</label>
              <input
                type="number"
                value={newSeats}
                min={1}
                onChange={(e) => setNewSeats(parseInt(e.target.value))}
                className="p-1 border rounded text-sm text-brown-500 ml-2"
              />

              <button
                onClick={addTable}
                className="p-1 bg-neutral-lattel-light text-brown-500 text-sm rounded px-4 ml-4"
              >
                {t("add_table")}
              </button>
            </div>

            <div>
              <button
                onClick={savePlaceGround}
                className={`p-1 bg-mocha-600 text-white text-sm rounded mt-5 px-4 ${btnVisibility}`}
              >
                {t("save")}
              </button>
            </div>
          </div>

          <div className="w-[750px] h-[550px] relative" id="place-ground">
            <div
              ref={containerRef}
              className="w-full h-full border border-gray-300 bg-gray-50"
              style={{
                backgroundImage: "url(../assets/images/place_view.png)",
                backgroundSize: "contain",
                backgroundRepeat: "no-repeat",
                position: "relative",
              }}
            >
              {tables.map((table, index) => (
                <Rnd
                  key={index}
                  bounds="parent"
                  position={{ x: table?.positionX ?? 10, y: table?.positionY ?? 10 }}
                  size={{ width: table?.width ?? 100, height: table.height ?? 100 }}
                  onMouseDown={(e) => {
                    console.log("mouseDown")
                    setMouseDownPosition({ x: e.clientX, y: e.clientY });
                  }}
                  onMouseUp={(e) => {
                    console.log("onMouseUp")
                    if (mouseDownPosition) {
                      const currentPos = { x: e.clientX, y: e.clientY };
                      if (!isSignificantDrag(mouseDownPosition, currentPos)) {
                        setSelectedTableIndex(index);
                        console.log("bbb "+index);
                      }
                      setMouseDownPosition(null);
                    }
                  }}
                  onDragStart={() => setIsDragging(true)}
                  onDragStop={(e, d) => {setTimeout(() => setIsDragging(false), 0); updateTable(index, d); setSelectedTableIndex(index); console.log("aa" + index)}}
                  onResizeStop={(e, direction, ref, delta, position) =>
                    updateTable(index, position, {
                      width: parseInt(ref.style.width),
                      height: parseInt(ref.style.height),
                    })
                  }
                  // onClick={(e:any) => {e.stopPropagation();  if (!isDragging){setSelectedTableIndex(index); console.log("bbb "+index)} }}
                  style={{
                    backgroundColor: getTableColor(table.status),
                    borderRadius: "50px",
                    color: "white",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    fontSize: "12px",
                    textAlign: "center",
                    cursor: "pointer",
                    position: "absolute",
                  }}
                >
                  <div className="relative" onClick={(e) => e.stopPropagation()}>
                    {table.label}

                    {selectedTableIndex === index && (
                      <div className="absolute -top-30 left-1/2 -translate-x-1/2 bg-white text-black text-xs rounded shadow p-2 z-50"
                           onClick={(e) => e.stopPropagation()}>
                        <button
                          onClick={() => deleteTable(index)}
                          className="block text-red-500 hover:underline mb-1"
                        >
                          {t("delete")}
                        </button>
                        <button
                          onClick={() => alert(`Generate QR for ${table.label}`)}
                          className="block text-black"
                        >
                          {t("generate_qr")}
                        </button>
                        <button
                          onClick={() => alert(`Generate QR for ${table.label}`)}
                          className="block text-blue-500"
                        >
                          {t("set_as_empty")}
                        </button>
                        <button
                          onClick={() => alert(`Generate QR for ${table.label}`)}
                          className="block text-blue-500"
                        >
                          {t("set_as_occupied")}
                        </button>
                        <button
                          onClick={() => alert(`Generate QR for ${table.label}`)}
                          className="block text-blue-500"
                        >
                          {t("set_as_reserved")}
                        </button>
                      </div>
                    )}
                  </div>
                </Rnd>
              ))}
            </div>
          </div>
        </>
      )}
      
      {screenWidth < minScreenSize && (
        <p>Upravljanje stolovima moguće je samo na desktop računalima.</p>
      )}

      
    </div>
  );
}

export default TableViewPage;
