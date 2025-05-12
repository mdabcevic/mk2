import { useTranslation } from "react-i18next";
import LoginForm from "./login-form";
import loginIllustration from "../../../public/assets/images/coffee_shop.png";

const LoginPage = () => {
  useTranslation("public");

  return (
    <div className="min-h-screen flex items-center justify-center bg-[#F5F0EA] px-4 py-8">
      {/* Main Card */}
      <div className="flex w-full max-w-4xl h-[520px] rounded-2xl shadow-lg overflow-hidden border border-[#d9cbb2] bg-white relative">
        {/* Left: Illustration */}
        <div className="w-1/2 hidden lg:block relative">
          <img
            src={loginIllustration}
            alt="Cafe illustration"
            className="w-full h-full object-cover"
          />
          <div className="absolute top-0 right-0 h-full w-[95px] z-10">
            <svg
              width="115.5"
              height="100%"
              viewBox="0 0 100 580"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                d="M74.01 102.312C56.7679 140.754 22.83 181.094 8.91552 219.998C-5.43771 260.031 -4.60168 301.787 9.23834 341.884C18.9479 370.032 34.9566 397.364 43.8881 425.59C60.2978 478.042 51.9704 532.133 19.8087 582H96.5V-2H76.4684C95.606 31.0009 89.3068 68.2211 74.01 102.312Z"
                fill="#E3DAC9"
              />
            </svg>
          </div>
        </div>

        {/* Right: Form Section */}
        <div className="w-full lg:w-1/2 bg-[#E3DAC9] px-10 py-12 flex items-center justify-center">
          <div className="w-full max-w-md">
            <LoginForm />
          </div>
        </div>
      </div>

      {/* Not implemented yet. It will be added in the future! */}
      {/* <p className="text-sm text-center text-gray-600 mt-4">
          Don’t have an account?{" "}
          <span className="text-[#f49241] hover:underline cursor-pointer">Sign up</span>
        </p> */}
    </div>
  );
};

export default LoginPage;
