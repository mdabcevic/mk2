import { Outlet } from "react-router-dom";
import HeaderComponent from "./header";

const Layout = () => {

  return (
    <>
      <HeaderComponent/>
      <main>
        <Outlet />
      </main>
       
      
    </>
  );
};

export default Layout;
