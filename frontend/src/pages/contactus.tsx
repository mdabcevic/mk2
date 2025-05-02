import { useTranslation } from "react-i18next";
import { useState } from "react";

function ContactUs() {
  const { t } = useTranslation("public");

  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    email: "",
    phoneNumber: "",
    message: ""
  });

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    console.log(formData); // Replace with your form submission logic
  };

  return (
    <div className="max-w-[1200px] mx-auto pt-10 pb-20 px-4">
      <h2 className="text-3xl font-semibold text-center text-gray-800 mb-8">
        {t("contact_us")}
      </h2>
      <form onSubmit={handleSubmit} className="bg-white shadow-md rounded-xl p-6">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
          <input
            type="text"
            name="firstName"
            placeholder={t("first_name")}
            value={formData.firstName}
            onChange={handleChange}
            className="border border-gray-300 rounded-lg p-2"
            required
          />
          <input
            type="text"
            name="lastName"
            placeholder={t("last_name")}
            value={formData.lastName}
            onChange={handleChange}
            className="border border-gray-300 rounded-lg p-2"
            required
          />
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
          <input
            type="email"
            name="email"
            placeholder={t("email")}
            value={formData.email}
            onChange={handleChange}
            className="border border-gray-300 rounded-lg p-2"
            required
          />
          <input
            type="tel"
            name="phoneNumber"
            placeholder={t("phone_number")}
            value={formData.phoneNumber}
            onChange={handleChange}
            className="border border-gray-300 rounded-lg p-2"
            required
          />
        </div>

        <textarea
          name="message"
          placeholder={t("your_message")}
          value={formData.message}
          onChange={handleChange}
          className="border border-gray-300 rounded-lg p-2 w-full mb-4 resize-none"
          rows={5}
          required
        />

        <button
          type="submit"
          className="bg-blue-600 hover:bg-blue-700 text-white rounded-lg py-2 px-4 w-full font-semibold transition duration-300"
        >
          {t("send_message")}
        </button>
      </form>
    </div>
  );
}

export default ContactUs;
