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
    const response = await tableService.getPlaceTablesByCurrentUser();
    setTables(response);
  };

  useEffect(() => {
    fetchTables();
  }, []);

  return (
    <div className="flex flex-col items-center p-4 space-y-6 ">
      {screenWidth >= minScreenSize && (
        <>
          <div className="flex flex-col w-full items-center" id="table-info">

            <div className="flex flex-col items-center justify-center max-w-[350px]">
              <div className="flex flex-row justify-between gap-4">
                <div className="flex flex-row w-[70%]">
                  <input
                    type="text"
                    value={newTableName}
                    onChange={(e) => setNewTableName(e.target.value)}
                    placeholder="Table"
                    className="pl-5 py-2 border rounded-[40px] text-sm w-full"
                  />
                  <span className="text-red-500">{labelMessage}</span>
                </div>

                <input
                  type="number"
                  value={newSeats}
                  placeholder="Seats"
                  min={1}
                  onChange={(e) => {if(e.target.value !== "") setNewSeats(parseInt(e.target.value)); else setNewSeats(0)}}
                  className=" pl-5 py-2 border rounded-[40px] text-sm w-[30%]"
                />
              </div>
              
              <div className="flex flex-row w-full gap-4 mt-2">
                <button
                  onClick={addTable}
                  className={`py-2 bg-mocha-500 text-white flex-1 font-bold text-sm rounded-[12px]`}
                >
                  {t("add_table").toUpperCase()}
                </button>
                <button
                  onClick={savePlaceGround}
                  className=" py-2 bg-white flex-1 text-brown-500 font-bold text-sm rounded-[12px] border-mocha"
                >
                  {t("save").toUpperCase()}
                </button>

              </div>          
            </div>

            
          </div>

          <div className={`relative`} style={{width:Constants.create_tables_container_width, height:Constants.create_tables_container_height}} id="place-ground">
            <div
              ref={containerRef}
              className="w-full h-full border border-gray-300 "
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
                  onDragStop={(_e, d) => {updateTable(index, d);}}
                  onResizeStop={(_e, _direction, ref, _delta, position) =>
                    updateTable(index, position, {
                      width: parseInt(ref.style.width),
                      height: parseInt(ref.style.height),
                    })
                  }
                  className="absolute bg-[#737373] rounded-[50px] text-white flex items-center justify-center text-[12px] cursor-pointer"
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
