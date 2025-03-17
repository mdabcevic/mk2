import { Outlet } from "react-router-dom";
import HeaderComponent from "./header";


const Layout = () => {
  return (
    <>
      <HeaderComponent />
      <main>
        <Outlet /> {/* This renders the matched route component */}
      </main>
    </>
  );
};

export default Layout;
