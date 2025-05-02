import  { forwardRef } from "react";
import ProductsSection from "./products-section";

const Products = forwardRef((_, ref) => {

    return (
    <div className="p-4">
      <ProductsSection ref={ref} />
    </div>
  );
});

export default Products;