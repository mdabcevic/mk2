import React from "react";
import { Link } from "react-router-dom";

type ButtonProps = {
  textValue: string;
  type: "brown" | "white";
  size: "small" | "medium" | "large";
  className?: string;
  onClick?: () => void;
  onClose?: () => void;
  navigateTo?: string;
};

export function Button({
  textValue,
  type,
  size,
  className = "",
  onClick,
  onClose,
  navigateTo,
}: ButtonProps) {

  const typeClasses =
    type === "brown"
      ? "bg-mocha-300 hover:bg-mocha-400 transition-colors rounded-md mt-6 w-[180px] h-[50px] text-white font-bold"
      : "bg-white text-black border border-gray-300";

  const sizeClasses = {
    small: "px-3 py-1 text-sm",
    medium: "px-4 py-2 text-base",
    large: "px-5 py-3 text-lg",
  }[size];

  const baseClass =`rounded-xl font-medium ${typeClasses} ${sizeClasses} ` + className;

  const handleClick = (_e: React.MouseEvent<HTMLButtonElement>) => {
    onClick?.();
    onClose?.();
  };

  const buttonElement = (
    <button onClick={handleClick} className={baseClass}>
      {textValue}
    </button>
  );

  return navigateTo ? (
    <Link to={navigateTo} className="inline-block">
      {buttonElement}
    </Link>
  ) : (
    buttonElement
  );
}
