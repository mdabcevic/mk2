import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Home from "./pages/home";
import NotFoundPage from "./pages/not-found-page";
import Layout from "./components/layout";
import AboutUs from "./pages/about-us";
import AdminLayout from "./admin/admin-layout";
import Dashboard from "./admin/pages/dashboard";
import MenuViewPage from "./admin/pages/menu-view";
import ProductsViewPage from "./admin/pages/products-view";
import TableViewPage from "./admin/pages/tables-view";
import PlaceDetails from "./pages/place-details";
import Menu from "./components/menu";


function AppRoutes(){
  return (
    <Router>
      <Routes>
        {/* Wrap all routes inside the Layout */}
        <Route path="/" element={<Layout />}>
          <Route index element={<Home />} />
          <Route path="aboutus" element={<AboutUs />} />
          <Route path="bar/:id" element={<PlaceDetails />} />
          <Route path="bar/:placeId/menu" element={<Menu />} />
          <Route path="*" element={<NotFoundPage />} />
        </Route>

        {/* Admin Routes (Using AdminLayout) */}
        <Route path="admin" element={<AdminLayout />}>
          <Route index element={<Dashboard />} />
          <Route path="menu" element={<MenuViewPage/>} />
          <Route path="products" element={<ProductsViewPage/>} />
          <Route path="tables" element={<TableViewPage/>} />
        </Route>
      </Routes>
    </Router>
  );
};

export default AppRoutes;