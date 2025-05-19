import { useTranslation } from "react-i18next";
import { Button } from "../../utils/components/button";

function ContactForm() {
  const { t } = useTranslation("public");
  return (
    <form className="flex flex-col gap-4 w-full max-w-[500px] lg:ml-10">
      <label>
        <span className="block mb-1">{t("contactUs.email")}</span>
        <input
          type="email"
          className="w-full px-4 py-2 border border-gray-400 rounded"
          placeholder="you@example.com"
        />
      </label>

      <label>
        <span className="block mb-1">{t("contactUs.subject")}</span>
        <input
          type="text"
          className="w-full px-4 py-2 border border-gray-400 rounded"
          placeholder={t("contactUs.subject")}
        />
      </label>

      <label>
        <span className="block mb-1">{t("contactUs.message")}</span>
        <textarea
          className="w-full px-4 py-2 border border-gray-400 rounded h-40 resize-none"
          placeholder={t("contactUs.message")}
        />
      </label>
      <Button 
        textValue={t("contactUs.send").toUpperCase()}
        type={"brown-dark"} 
        size={"small"}
        className={"w-fit px-6 py-2 mt-2 rounded"}
      />
    </form>
  );
}

export default ContactForm;
