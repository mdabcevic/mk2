import { useState, useRef, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Rnd } from "react-rnd";
import { tableService } from "../../../utils/services/tables.service";
import { Constants, Table, TableStatusString } from "../../../utils/constants";

const screenWidth = window.innerWidth;
const minScreenSize = 850;

function CreateTables() {
  const containerRef = useRef<HTMLDivElement>(null);
  const { t } = useTranslation("admin");
  const [tables, setTables] = useState<Table[]>([]);
  const [newTableName, setNewTableName] = useState<string>("");
  const [newSeats, setNewSeats] = useState<number>(4);
  const [labelMessage, setLabelMessage] = useState<string>("");
  
  const addTable = () => {
    if (!newTableName.trim()) return;

    if (tables.find((table) => table.label === newTableName)) {
      setLabelMessage(t("table_already_exist"));
      return;
    }

    const newTable: Table = {
      label: newTableName,
      x: 0,
      y: 0,
      width: 100,
      height: 100,
      seats: newSeats,
      status: TableStatusString.empty,
    };

    setTables((prev) => [...prev, newTable]);
    setNewTableName("");
    setNewSeats(4);
    setLabelMessage("");
  };

  const updateTable = (index: number, position: any, size?: any) => {
    setTables((prevTables) => {
      const updated = [...prevTables];
      updated[index] = {
        ...updated[index],
        x: position.x,
        y: position.y,
        ...(size && {
          width: size.width,
          height: size.height,
        }),
      };
      return updated;
    });
  };

  const savePlaceGround = async () => {
    await tableService.saveOrUpdateTables(tables);
  };

  const fetchTables = async () => {
    const response = await tableService.getPlaceTablesByCurrent();
    setTables(response);
  };

  useEffect(() => {
    fetchTables();
  }, []);

  return (
    <div className="flex flex-col items-center p-4 space-y-6 ">
      {screenWidth >= minScreenSize && (
        <>
          <div className="flex flex-col w-full" id="table-info">
            <h2 className="text-base font-semibold text-left text-light">{t("table_info")}</h2>
            <div className="flex flex-row items-center">
              <label className="text-light">{t("table_label")}:</label>
              <div className="flex flex-col ml-2 mr-4">
                <input
                  type="text"
                  value={newTableName}
                  onChange={(e) => setNewTableName(e.target.value)}
                  placeholder="753-23"
                  className="p-1 border rounded text-sm text-light"
                />
                <span className="text-red-500">{labelMessage}</span>
              </div>

              <label className="text-light">{t("seats_number")}:</label>
              <input
                type="number"
                value={newSeats}
                min={1}
                onChange={(e) => setNewSeats(parseInt(e.target.value))}
                className="p-1 border rounded text-sm text-light ml-2"
              />

              <button
                onClick={addTable}
                className="p-1 bg-neutral-latte-light text-brown-500 text-sm rounded px-4 ml-4"
              >
                {t("add_table")}
              </button>
            </div>

            <div>
              <button
                onClick={savePlaceGround}
                className={`p-1 bg-mocha-600 text-white text-sm rounded mt-5 px-4`}
              >
                {t("save")}
              </button>
            </div>
          </div>

          <div className={`relative`} style={{width:Constants.create_tables_container_width, height:Constants.create_tables_container_height}} id="place-ground">
            <div
              ref={containerRef}
              className="w-full h-full border border-gray-300 bg-gray-50"
              style={{
                backgroundImage: `url(../${Constants.template_image})`,
                backgroundSize: "contain",
                backgroundRepeat: "no-repeat",
                position: "relative",
              }}
            >
              {tables.map((table, index) => (
                <Rnd
                  key={index}
                  bounds="parent"
                  position={{ x: table?.x ?? 10, y: table?.y ?? 10 }}
                  size={{ width: table?.width ?? 100, height: table.height ?? 100 }}
                  onDragStop={(e, d) => {updateTable(index, d);}}
                  onResizeStop={(e, direction, ref, delta, position) =>
                    updateTable(index, position, {
                      width: parseInt(ref.style.width),
                      height: parseInt(ref.style.height),
                    })
                  }
                  className="absolute bg-mocha-300 rounded-[50px] text-white flex items-center justify-center text-[12px] cursor-pointer"
                >
                  <div className="w-full h-full flex items-center justify-center" onClick={(e) => e.stopPropagation()}>
                    {table.label}
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

export default CreateTables;
