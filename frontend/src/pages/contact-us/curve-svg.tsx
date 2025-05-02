function CurveSvg() {
    return (
      <div className="absolute top-0 right-0 h-full w-[134px] -mr-[2px] z-20">
        {/* ...SVG content... */}
        <svg
            viewBox="0 0 132 714"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
            preserveAspectRatio="none"
            className="h-full w-full"
          >
            <g clipPath="url(#clip0_932_2909)">
              <path
                d="M65.8503 714L68.0366 674.333C70.3878 634.667 74.5128 555.333 63.6641 476C52.6503 396.667 26.2503 317.333 30.6641 238C34.9128 158.667 70.3878 79.3333 87.8366 39.6667L105.45 0H131.85V39.6667C131.85 79.3333 131.85 158.667 131.85 238C131.85 317.333 131.85 396.667 131.85 476C131.85 555.333 131.85 634.667 131.85 674.333V714H65.8503Z"
                fill="#F6F2EC"
              />
            </g>
            <defs>
              <clipPath id="clip0_932_2909">
                <rect
                  width="714"
                  height="132"
                  fill="white"
                  transform="matrix(0 -1 1 0 0 714)"
                />
              </clipPath>
            </defs>
          </svg>
      </div>
    );
  }
  
  export default CurveSvg;