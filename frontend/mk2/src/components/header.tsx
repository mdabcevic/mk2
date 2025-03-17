import { Link } from "react-router-dom";

const HeaderComponent = () => {
  return (
    <header>
      <h1>My Website</h1>
      <nav>
        <Link to="/home">Home</Link> | <Link to="/NotFound">Not Found</Link>
      </nav>
    </header>
  );
};

export default HeaderComponent;
