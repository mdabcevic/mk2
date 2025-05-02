import ContactInfo from "./contact-info";

function ContactUsPage() {
  return (
    <div className="flex h-screen overflow-hidden relative p-0 m-0">
      {/* Left Panel */}
      <div className="relative w-1/3 bg-[#432B1F] text-white z-10">
        <div className="pl-35 flex flex-col justify-center h-full">
          <ContactInfo />
        </div>

        {/* SVG cuts into right panel */}
        <div className="absolute top-0 right-0 h-full w-[134px] -mr-[2px] z-20">
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
      </div>

      {/* Right Panel */}
      <div className="w-2/3 bg-[#F5F0EA] p-10 relative z-10 flex justify-center items-center h-full">
        <div className="flex flex-col lg:flex-row gap-10 items-center justify-center w-full max-w-5xl">
          {/* Form */}
          <form className="flex flex-col gap-4 w-full max-w-md">
            <label>
              <span className="block mb-1">Email</span>
              <input
                type="email"
                className="w-full px-4 py-2 border border-gray-400 rounded"
                placeholder="you@example.com"
              />
            </label>

            <label>
              <span className="block mb-1">Subject</span>
              <input
                type="text"
                className="w-full px-4 py-2 border border-gray-400 rounded"
                placeholder="Subject"
              />
            </label>

            <label>
              <span className="block mb-1">Message</span>
              <textarea
                className="w-full px-4 py-2 border border-gray-400 rounded h-40 resize-none"
                placeholder="Your message..."
              />
            </label>

            <button
              type="submit"
              className="bg-[#432B1F] text-white px-6 py-2 rounded w-fit mt-2"
            >
              SEND
            </button>
          </form>

          {/* Map */}
          <div className="w-[240px] h-[240px] rounded-full overflow-hidden border border-gray-300 shrink-0">
            <iframe
              src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d2781.997272749197!2d15.984169176508398!3d45.801651971082915!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x4765d6f1d8c155fb%3A0x88e4871cd46a6d49!2sUl.%20Vjekoslava%20Heinzela%2C%20Zagreb!5e0!3m2!1sen!2shr!4v1717168550123!5m2!1sen!2shr"
              width="100%"
              height="100%"
              style={{ border: 0 }}
              allowFullScreen
              loading="lazy"
              referrerPolicy="no-referrer-when-downgrade"
            ></iframe>
          </div>
        </div>
      </div>
    </div>
  );
}

export default ContactUsPage;
