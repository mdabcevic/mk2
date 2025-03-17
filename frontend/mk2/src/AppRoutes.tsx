import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Home from "./components/home";
import NotFoundPage from "./components/not-found-page";
import Layout from "./components/layout";


const AppRoutes = () => {
  return (
    <Router>
      <Routes>
        {/* Wrap all routes inside the Layout */}
        <Route path="/" element={<Layout />}>
          <Route path="home" element={<Home />} />
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Routes>
    </Router>
  );
};

export default AppRoutes;